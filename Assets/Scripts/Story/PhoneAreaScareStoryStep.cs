using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace HorrorCafe.Story
{
    public sealed class PhoneAreaScareStoryStep : StoryStep
    {
        [Header("Triggers")]
        [SerializeField] private StoryTriggerZone prePhoneTrigger;
        [SerializeField] private StoryTriggerZone phoneTrigger;

        [Header("Phone")]
        [SerializeField] private AudioSource phoneAudioSource;
        [SerializeField] private DialogueLine[] linesOnPhoneStop;

        [Header("Light")]
        [SerializeField] private Light[] flickerLights;
        [SerializeField, Min(0.03f)] private float flickerInterval = 0.08f;
        [SerializeField, Range(0f, 1f)] private float flickerLowMultiplier = 0.15f;
        [SerializeField] private bool restoreLightWhenMonsterGone = true;

        [Header("Cop")]
        [SerializeField] private GameObject copObject;

        [Header("Monster")]
        [SerializeField] private GameObject monsterObject;
        [SerializeField] private Transform monsterExitPoint;
        [SerializeField, Min(0.05f)] private float monsterMoveSpeed = 1.6f;
        [SerializeField, Min(0.01f)] private float monsterArriveDistance = 0.15f;
        [SerializeField] private Animator monsterAnimator;
        [SerializeField] private string monsterWalkingBoolName = "IsWalking";

        [Header("Look")]
        [SerializeField] private Camera playerCamera;
        [SerializeField, Range(1f, 90f)] private float viewAngle = 35f;
        [SerializeField] private bool requireLineOfSight;
        [SerializeField] private LayerMask lineOfSightMask = ~0;

        [Header("Audio")]
        [SerializeField] private AudioSource heartbeatAudioSource;
        [SerializeField] private AudioClip heartbeatClip;
        [SerializeField] private AudioSource scareAudioSource;
        [SerializeField] private AudioClip scareClip;
        [SerializeField, Min(0f)] private float audioFadeOutDuration = 0.75f;

        [Header("Dialogue")]
        [SerializeField] private DialogueLine[] linesAfterMonsterGone;

        private bool prePhoneEntered;
        private bool phoneEntered;
        private Coroutine flickerRoutine;
        private float[] originalLightIntensities;

        protected override IEnumerator Execute(StoryScenarioController controller)
        {
            if (prePhoneTrigger == null || phoneTrigger == null)
            {
                Debug.LogWarning("Phone area scare needs both trigger zones.", this);
                yield break;
            }

            prePhoneEntered = false;
            phoneEntered = false;

            PrepareTrigger(prePhoneTrigger, OnPrePhoneEntered);
            PrepareTrigger(phoneTrigger, OnPhoneEntered);

            yield return WaitForPrePhone();
            yield return WaitForPhone(controller);
            yield return RunMonsterScare(controller);

            CleanupTrigger(prePhoneTrigger, OnPrePhoneEntered);
            CleanupTrigger(phoneTrigger, OnPhoneEntered);
        }

        private IEnumerator WaitForPrePhone()
        {
            while (!prePhoneEntered)
                yield return null;

            StartLightFlicker();

            if (copObject != null)
                copObject.SetActive(false);
        }

        private IEnumerator WaitForPhone(StoryScenarioController controller)
        {
            while (!phoneEntered)
                yield return null;

            if (phoneAudioSource != null)
            {
                phoneAudioSource.Stop();
                phoneAudioSource.loop = false;
            }

            yield return PlayDialogue(controller, linesOnPhoneStop);
        }

        private IEnumerator RunMonsterScare(StoryScenarioController controller)
        {
            if (monsterObject == null)
                yield break;

            monsterObject.SetActive(true);

            while (!IsMonsterVisible())
                yield return null;

            PlayLoop(heartbeatAudioSource, heartbeatClip);
            PlayOneShot(scareAudioSource, scareClip);

            if (monsterExitPoint != null)
            {
                SetMonsterWalking(true);
                yield return MoveMonsterToExit();
                SetMonsterWalking(false);
            }

            monsterObject.SetActive(false);

            if (restoreLightWhenMonsterGone)
                StopLightFlicker();

            yield return FadeOutAudioTogether(audioFadeOutDuration, heartbeatAudioSource, scareAudioSource);
            yield return PlayDialogue(controller, linesAfterMonsterGone);
        }

        private void PrepareTrigger(StoryTriggerZone trigger, System.Action handler)
        {
            trigger.ResetTrigger();
            trigger.Entered -= handler;
            trigger.Entered += handler;
        }

        private void CleanupTrigger(StoryTriggerZone trigger, System.Action handler)
        {
            if (trigger != null)
                trigger.Entered -= handler;
        }

        private void OnDisable()
        {
            CleanupTrigger(prePhoneTrigger, OnPrePhoneEntered);
            CleanupTrigger(phoneTrigger, OnPhoneEntered);
            StopLightFlicker();
        }

        private void OnPrePhoneEntered()
        {
            prePhoneEntered = true;
        }

        private void OnPhoneEntered()
        {
            phoneEntered = true;
        }

        private IEnumerator PlayDialogue(StoryScenarioController controller, DialogueLine[] lines)
        {
            if (controller.DialogueRunner == null || lines == null || lines.Length == 0)
                yield break;

            controller.DialogueRunner.Play(this, lines);
            while (controller.DialogueRunner.IsPlaying)
                yield return null;
        }

        private bool IsMonsterVisible()
        {
            if (playerCamera == null || monsterObject == null)
                return true;

            var target = monsterObject.transform;
            var toTarget = target.position - playerCamera.transform.position;
            var angle = Vector3.Angle(playerCamera.transform.forward, toTarget.normalized);
            var visible = angle <= viewAngle;

            if (visible && requireLineOfSight)
                visible = HasLineOfSight(target, toTarget);

            return visible;
        }

        private bool HasLineOfSight(Transform target, Vector3 toTarget)
        {
            var ray = new Ray(playerCamera.transform.position, toTarget.normalized);
            if (!Physics.Raycast(ray, out var hit, toTarget.magnitude, lineOfSightMask, QueryTriggerInteraction.Ignore))
                return true;

            return hit.transform == target || hit.transform.IsChildOf(target);
        }

        private IEnumerator MoveMonsterToExit()
        {
            var monsterTransform = monsterObject.transform;
            var agent = monsterObject.GetComponent<NavMeshAgent>();
            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.SetDestination(monsterExitPoint.position);

                while (agent.pathPending || agent.remainingDistance > monsterArriveDistance)
                    yield return null;

                agent.isStopped = true;
                yield break;
            }

            while (Vector3.Distance(monsterTransform.position, monsterExitPoint.position) > monsterArriveDistance)
            {
                monsterTransform.position = Vector3.MoveTowards(monsterTransform.position, monsterExitPoint.position, monsterMoveSpeed * Time.deltaTime);

                var direction = monsterExitPoint.position - monsterTransform.position;
                direction.y = 0f;
                if (direction.sqrMagnitude > 0.0001f)
                    monsterTransform.rotation = Quaternion.Slerp(monsterTransform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 8f);

                yield return null;
            }
        }

        private void SetMonsterWalking(bool value)
        {
            if (monsterAnimator == null || string.IsNullOrWhiteSpace(monsterWalkingBoolName))
                return;

            monsterAnimator.SetBool(monsterWalkingBoolName, value);
        }

        private void StartLightFlicker()
        {
            if (flickerLights == null || flickerLights.Length == 0 || flickerRoutine != null)
                return;

            originalLightIntensities = new float[flickerLights.Length];
            for (var i = 0; i < flickerLights.Length; i++)
            {
                if (flickerLights[i] != null)
                    originalLightIntensities[i] = flickerLights[i].intensity;
            }

            flickerRoutine = StartCoroutine(FlickerLightRoutine());
        }

        private void StopLightFlicker()
        {
            if (flickerRoutine != null)
            {
                StopCoroutine(flickerRoutine);
                flickerRoutine = null;
            }

            RestoreFlickerLights();
        }

        private IEnumerator FlickerLightRoutine()
        {
            while (true)
            {
                SetFlickerMultiplier(flickerLowMultiplier);
                yield return new WaitForSeconds(flickerInterval);
                SetFlickerMultiplier(1f);
                yield return new WaitForSeconds(flickerInterval);
            }
        }

        private void SetFlickerMultiplier(float multiplier)
        {
            if (flickerLights == null || originalLightIntensities == null)
                return;

            for (var i = 0; i < flickerLights.Length && i < originalLightIntensities.Length; i++)
            {
                if (flickerLights[i] != null)
                    flickerLights[i].intensity = originalLightIntensities[i] * multiplier;
            }
        }

        private void RestoreFlickerLights()
        {
            if (flickerLights == null || originalLightIntensities == null)
                return;

            for (var i = 0; i < flickerLights.Length && i < originalLightIntensities.Length; i++)
            {
                if (flickerLights[i] != null)
                    flickerLights[i].intensity = originalLightIntensities[i];
            }
        }

        private static void PlayLoop(AudioSource source, AudioClip clip)
        {
            if (source == null || clip == null)
                return;

            source.clip = clip;
            source.loop = true;
            source.volume = 1f;
            source.Play();
        }

        private static void PlayOneShot(AudioSource source, AudioClip clip)
        {
            if (source != null && clip != null)
                source.PlayOneShot(clip);
        }

        private static IEnumerator FadeOutAudioTogether(float duration, params AudioSource[] sources)
        {
            if (sources == null || sources.Length == 0)
                yield break;

            if (duration <= 0f)
            {
                StopSources(sources);
                yield break;
            }

            var startVolumes = new float[sources.Length];
            for (var i = 0; i < sources.Length; i++)
                startVolumes[i] = sources[i] != null ? sources[i].volume : 0f;

            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / duration;
                for (var i = 0; i < sources.Length; i++)
                {
                    if (sources[i] != null && sources[i].isPlaying)
                        sources[i].volume = Mathf.Lerp(startVolumes[i], 0f, t);
                }

                yield return null;
            }

            for (var i = 0; i < sources.Length; i++)
            {
                if (sources[i] == null)
                    continue;

                sources[i].Stop();
                sources[i].loop = false;
                sources[i].volume = startVolumes[i];
            }
        }

        private static void StopSources(AudioSource[] sources)
        {
            foreach (var source in sources)
            {
                if (source != null)
                {
                    source.Stop();
                    source.loop = false;
                }
            }
        }
    }
}
