using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace HorrorCafe.Story
{
    public sealed class NpcMoveStoryStep : StoryStep
    {
        [SerializeField] private Transform npc;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform targetPoint;
        [SerializeField, Min(0.05f)] private float fallbackMoveSpeed = 1.4f;
        [SerializeField, Min(0.01f)] private float arriveDistance = 0.15f;
        [SerializeField] private bool activateNpc = true;
        [SerializeField] private bool faceTargetOnArrive = true;
        [SerializeField] private bool deactivateNpcOnArrive;
        [SerializeField] private Animator animator;
        [SerializeField] private string walkingBoolName = "IsWalking";
        [SerializeField] private AudioSource startAudioSource;
        [SerializeField] private AudioClip startClip;
        [SerializeField] private DialogueLine[] linesOnArrive;

        protected override IEnumerator Execute(StoryScenarioController controller)
        {
            if (npc == null)
                yield break;

            if (spawnPoint != null)
                npc.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);

            if (activateNpc)
                npc.gameObject.SetActive(true);

            PlayStartSound();

            if (targetPoint != null)
            {
                SetWalking(true);
                yield return MoveToTarget();
                SetWalking(false);
            }

            if (faceTargetOnArrive && targetPoint != null)
                npc.rotation = targetPoint.rotation;

            if (controller.DialogueRunner != null && linesOnArrive != null && linesOnArrive.Length > 0)
            {
                controller.DialogueRunner.Play(this, linesOnArrive);
                while (controller.DialogueRunner.IsPlaying)
                    yield return null;
            }

            if (deactivateNpcOnArrive)
                npc.gameObject.SetActive(false);
        }

        private IEnumerator MoveToTarget()
        {
            var agent = npc.GetComponent<NavMeshAgent>();
            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.SetDestination(targetPoint.position);

                while (agent.pathPending || agent.remainingDistance > arriveDistance)
                    yield return null;

                agent.isStopped = true;
                yield break;
            }

            while (Vector3.Distance(npc.position, targetPoint.position) > arriveDistance)
            {
                npc.position = Vector3.MoveTowards(npc.position, targetPoint.position, fallbackMoveSpeed * Time.deltaTime);

                var direction = targetPoint.position - npc.position;
                direction.y = 0f;
                if (direction.sqrMagnitude > 0.0001f)
                    npc.rotation = Quaternion.Slerp(npc.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 8f);

                yield return null;
            }
        }

        private void SetWalking(bool value)
        {
            if (animator == null && npc != null)
                animator = npc.GetComponentInChildren<Animator>(true);

            if (animator == null || string.IsNullOrWhiteSpace(walkingBoolName))
                return;

            animator.SetBool(walkingBoolName, value);
        }

        private void PlayStartSound()
        {
            if (startAudioSource == null || startClip == null)
                return;

            startAudioSource.loop = false;
            startAudioSource.PlayOneShot(startClip);
        }
    }
}
