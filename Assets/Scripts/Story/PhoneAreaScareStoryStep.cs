using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace HorrorCafe.Story
{
    public sealed class PhoneAreaScareStoryStep : StoryStep
    {
        [System.Serializable]
        private struct FlickerStep
        {
            public float multiplier;
            public float duration;
        }
        
        [System.Serializable]
        private sealed class FlickerLightTarget
        {
            public Light light;

            [Min(0f)]
            public float startDelay;

            [System.NonSerialized] public float originalIntensity;
            [System.NonSerialized] public Coroutine routine;
            [System.NonSerialized] public bool hasOriginalIntensity;
        }

        [SerializeField] private FlickerStep[] flickerPattern =
        {
            new FlickerStep { multiplier = 1.0f, duration = 0.4f },
            new FlickerStep { multiplier = 0.35f, duration = 0.10f },
            new FlickerStep { multiplier = 1.15f, duration = 0.05f },
            new FlickerStep { multiplier = 0.02f, duration = 0.45f },
            new FlickerStep { multiplier = 0.8f, duration = 0.07f },
            new FlickerStep { multiplier = 0.05f, duration = 0.25f },
            new FlickerStep { multiplier = 1.1f, duration = 0.04f },
            new FlickerStep { multiplier = 0.03f, duration = 0.35f },
            new FlickerStep { multiplier = 0.6f, duration = 0.12f },
            new FlickerStep { multiplier = 1.0f, duration = 0.4f },
        };
        
        [Header("Light")]
        [SerializeField] private FlickerLightTarget[] flickerLightTargets;
        [SerializeField] private bool restoreLightWhenMonsterGone = true;

        [Header("Triggers")]
        [SerializeField] private StoryTriggerZone prePhoneTrigger;
        [SerializeField] private StoryTriggerZone phoneTrigger;

        [Header("Phone")]
        [SerializeField] private AudioSource phoneAudioSource;
        [SerializeField] private DialogueLine[] linesOnPhoneStop;

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
        private bool lightFlickerStarted;

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
            if (lightFlickerStarted || flickerLightTargets == null || flickerLightTargets.Length == 0)
                return;

            var startedAny = false;

            for (var i = 0; i < flickerLightTargets.Length; i++)
            {
                var targetLight = flickerLightTargets[i];

                if (targetLight == null || targetLight.light == null)
                    continue;

                targetLight.originalIntensity = targetLight.light.intensity;
                targetLight.hasOriginalIntensity = true;
                targetLight.light.enabled = true;

                targetLight.routine = StartCoroutine(FlickerSingleLightRoutine(targetLight));
                startedAny = true;
            }

            lightFlickerStarted = startedAny;
        }

        private void StopLightFlicker()
        {
            if (flickerLightTargets == null)
            {
                lightFlickerStarted = false;
                return;
            }

            for (var i = 0; i < flickerLightTargets.Length; i++)
            {
                var targetLight = flickerLightTargets[i];

                if (targetLight == null)
                    continue;

                if (targetLight.routine != null)
                {
                    StopCoroutine(targetLight.routine);
                    targetLight.routine = null;
                }

                if (targetLight.light != null && targetLight.hasOriginalIntensity)
                {
                    targetLight.light.enabled = true;
                    targetLight.light.intensity = targetLight.originalIntensity;
                }

                targetLight.hasOriginalIntensity = false;
            }

            lightFlickerStarted = false;
        }

        private IEnumerator FlickerSingleLightRoutine(FlickerLightTarget targetLight)
        {
            if (targetLight.startDelay > 0f)
                yield return new WaitForSeconds(targetLight.startDelay);

            while (true)
            {
                if (flickerPattern == null || flickerPattern.Length == 0)
                {
                    SetSingleLightMultiplier(targetLight, 1f);
                    yield return null;
                    continue;
                }

                for (var i = 0; i < flickerPattern.Length; i++)
                {
                    SetSingleLightMultiplier(targetLight, flickerPattern[i].multiplier);
                    yield return new WaitForSeconds(Mathf.Max(0.01f, flickerPattern[i].duration));
                }
            }
        }

        private void SetSingleLightMultiplier(FlickerLightTarget targetLight, float multiplier)
        {
            if (targetLight == null || targetLight.light == null || !targetLight.hasOriginalIntensity)
                return;

            targetLight.light.enabled = true;
            targetLight.light.intensity = targetLight.originalIntensity * multiplier;
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
