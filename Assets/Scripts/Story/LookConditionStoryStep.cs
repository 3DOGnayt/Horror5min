using System.Collections;
using UnityEngine;

namespace HorrorCafe.Story
{
    public sealed class LookConditionStoryStep : StoryStep
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Transform target;
        [SerializeField] private GameObject monsterObject;
        [SerializeField] private Transform monsterSpawnPoint;
        [SerializeField] private Light[] flickerLights;
        [SerializeField, Min(0.03f)] private float flickerInterval = 0.08f;
        [SerializeField, Range(0f, 1f)] private float flickerLowMultiplier = 0.15f;
        [SerializeField] private AudioSource growlAudioSource;
        [SerializeField] private AudioClip growlClip;
        [SerializeField] private AudioSource backgroundAudioSource;
        [SerializeField] private AudioClip backgroundClip;
        [SerializeField] private bool requireLooking = true;
        [SerializeField, Range(1f, 90f)] private float viewAngle = 25f;
        [SerializeField, Min(0f)] private float requiredDuration;
        [SerializeField] private LayerMask lineOfSightMask = ~0;
        [SerializeField] private bool requireLineOfSight;
        [SerializeField] private DialogueLine[] linesAfter;

        private float conditionTime;
        private Coroutine flickerRoutine;
        private float[] originalLightIntensities;

        private void Reset()
        {
            playerCamera = Camera.main;
        }

        private void OnDisable()
        {
            StopLightFlicker();
        }

        protected override IEnumerator Execute(StoryScenarioController controller)
        {
            conditionTime = 0f;

            ShowMonster();
            PlayGrowl();
            StartLightFlicker();

            var targetDuration = Mathf.Max(0.02f, requiredDuration);

            while (conditionTime < targetDuration)
            {
                if (MatchesCondition())
                    conditionTime += Time.deltaTime;
                else
                    conditionTime = 0f;

                yield return null;
            }

            if (controller.DialogueRunner != null && linesAfter != null && linesAfter.Length > 0)
            {
                PlayBackgroundLoop();
                controller.DialogueRunner.Play(this, linesAfter);
                while (controller.DialogueRunner.IsPlaying)
                    yield return null;
            }
        }

        private void ShowMonster()
        {
            if (monsterObject == null)
                return;

            var spawnPoint = monsterSpawnPoint != null ? monsterSpawnPoint : target;
            if (spawnPoint != null)
                monsterObject.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);

            monsterObject.SetActive(true);
        }

        private void PlayGrowl()
        {
            if (growlAudioSource == null || growlClip == null)
                return;

            growlAudioSource.loop = false;
            growlAudioSource.PlayOneShot(growlClip);
        }

        private void PlayBackgroundLoop()
        {
            if (backgroundAudioSource == null || backgroundClip == null)
                return;

            backgroundAudioSource.clip = backgroundClip;
            backgroundAudioSource.loop = true;
            backgroundAudioSource.Play();
        }

        private bool MatchesCondition()
        {
            if (playerCamera == null || target == null)
                return true;

            var toTarget = target.position - playerCamera.transform.position;
            var angle = Vector3.Angle(playerCamera.transform.forward, toTarget.normalized);
            var isLooking = angle <= viewAngle;

            if (isLooking && requireLineOfSight)
                isLooking = HasLineOfSight(toTarget);

            return requireLooking ? isLooking : !isLooking;
        }

        private bool HasLineOfSight(Vector3 toTarget)
        {
            var ray = new Ray(playerCamera.transform.position, toTarget.normalized);
            if (!Physics.Raycast(ray, out var hit, toTarget.magnitude, lineOfSightMask, QueryTriggerInteraction.Ignore))
                return true;

            return hit.transform == target || hit.transform.IsChildOf(target);
        }

        private void StartLightFlicker()
        {
            if (flickerRoutine != null || flickerLights == null || flickerLights.Length == 0)
                return;

            originalLightIntensities = new float[flickerLights.Length];
            for (var i = 0; i < flickerLights.Length; i++)
            {
                if (flickerLights[i] != null)
                    originalLightIntensities[i] = flickerLights[i].intensity;
            }

            flickerRoutine = StartCoroutine(FlickerLightsRoutine());
        }

        private void StopLightFlicker()
        {
            if (flickerRoutine != null)
            {
                StopCoroutine(flickerRoutine);
                flickerRoutine = null;
            }

            if (flickerLights == null || originalLightIntensities == null)
                return;

            for (var i = 0; i < flickerLights.Length && i < originalLightIntensities.Length; i++)
            {
                if (flickerLights[i] != null)
                    flickerLights[i].intensity = originalLightIntensities[i];
            }
        }

        private IEnumerator FlickerLightsRoutine()
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
    }
}
