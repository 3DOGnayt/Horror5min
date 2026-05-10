using System.Collections;
using UnityEngine;

namespace HorrorCafe.Story
{
    public sealed class LookConditionStoryStep : StoryStep
    {
        [System.Serializable]
        private struct FlickerStep
        {
            public float multiplier;
            public float duration;
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

        [SerializeField] private FlickerLightTarget[] flickerLightTargets;
        
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Transform target;
        [SerializeField] private GameObject monsterObject;
        [SerializeField] private Transform monsterSpawnPoint;
        
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
        private bool lightFlickerStarted;

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
                return;

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
    }
}
