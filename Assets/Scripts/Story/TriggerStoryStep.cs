using System.Collections;
using UnityEngine;

namespace HorrorCafe.Story
{
    [RequireComponent(typeof(Collider))]
    public sealed class TriggerStoryStep : StoryStep
    {
        [SerializeField] private bool requirePlayerTag = true;
        [SerializeField] private string playerTag = "Player";
        [SerializeField, Min(0f)] private float delayAfterTrigger;
        [SerializeField] private DialogueLine[] linesAfterTrigger;

        private bool triggered;

        private void Reset()
        {
            var triggerCollider = GetComponent<Collider>();
            if (triggerCollider != null)
                triggerCollider.isTrigger = true;
        }

        private void OnValidate()
        {
            var triggerCollider = GetComponent<Collider>();
            if (triggerCollider != null)
                triggerCollider.isTrigger = true;
        }

        private void OnTriggerStay(Collider other)
        {
            if (!IsRunning || triggered || other == null)
                return;

            if (requirePlayerTag && !other.CompareTag(playerTag))
                return;

            triggered = true;
        }

        protected override IEnumerator Execute(StoryScenarioController controller)
        {
            triggered = false;

            while (!triggered)
                yield return null;

            if (delayAfterTrigger > 0f)
                yield return new WaitForSeconds(delayAfterTrigger);

            if (controller.DialogueRunner != null && linesAfterTrigger != null && linesAfterTrigger.Length > 0)
            {
                controller.DialogueRunner.Play(this, linesAfterTrigger);
                while (controller.DialogueRunner.IsPlaying)
                    yield return null;
            }
        }
    }
}
