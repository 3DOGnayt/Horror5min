using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace HorrorCafe.Story
{
    public sealed class CopLeavePhoneRingStoryStep : StoryStep
    {
        [Header("NPC")]
        [SerializeField] private Transform npc;
        [SerializeField] private Transform exitPoint;
        [SerializeField, Min(0.05f)] private float fallbackMoveSpeed = 1.4f;
        [SerializeField, Min(0.01f)] private float arriveDistance = 0.15f;
        [SerializeField] private bool faceExitPointOnArrive = true;
        [SerializeField] private Animator animator;
        [SerializeField] private string walkingBoolName = "IsWalking";

        [Header("Phone")]
        [SerializeField] private AudioSource phoneAudioSource;
        [SerializeField] private AudioClip phoneRingClip;
        [SerializeField, Min(0f)] private float phoneRingDelay = 0.5f;
        [SerializeField] private bool loopPhoneRing = true;

        protected override IEnumerator Execute(StoryScenarioController controller)
        {
            if (npc == null || exitPoint == null)
            {
                Debug.LogWarning("Cop leave step needs NPC and exit point.", this);
                yield break;
            }

            StartCoroutine(StartPhoneRingAfterDelay());

            SetWalking(true);
            yield return MoveToExit();
            SetWalking(false);

            if (faceExitPointOnArrive)
                npc.rotation = exitPoint.rotation;
        }

        private IEnumerator StartPhoneRingAfterDelay()
        {
            if (phoneRingDelay > 0f)
                yield return new WaitForSeconds(phoneRingDelay);

            if (phoneAudioSource == null || phoneRingClip == null)
                yield break;

            phoneAudioSource.clip = phoneRingClip;
            phoneAudioSource.loop = loopPhoneRing;
            phoneAudioSource.Play();
        }

        private IEnumerator MoveToExit()
        {
            var agent = npc.GetComponent<NavMeshAgent>();
            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.SetDestination(exitPoint.position);

                while (agent.pathPending || agent.remainingDistance > arriveDistance)
                    yield return null;

                agent.isStopped = true;
                yield break;
            }

            while (Vector3.Distance(npc.position, exitPoint.position) > arriveDistance)
            {
                npc.position = Vector3.MoveTowards(npc.position, exitPoint.position, fallbackMoveSpeed * Time.deltaTime);

                var direction = exitPoint.position - npc.position;
                direction.y = 0f;
                if (direction.sqrMagnitude > 0.0001f)
                    npc.rotation = Quaternion.Slerp(npc.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 8f);

                yield return null;
            }
        }

        private void SetWalking(bool value)
        {
            if (animator == null || string.IsNullOrWhiteSpace(walkingBoolName))
                return;

            animator.SetBool(walkingBoolName, value);
        }
    }
}
