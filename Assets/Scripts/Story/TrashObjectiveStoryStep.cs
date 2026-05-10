using System.Collections;
using HorrorCafe.Interaction;
using UnityEngine;

namespace HorrorCafe.Story
{
    public sealed class TrashObjectiveStoryStep : StoryStep
    {
        [SerializeField] private TrashBinTrigger trashTrigger;
        [SerializeField] private DialogueLine[] linesBefore;
        [SerializeField] private DialogueLine[] linesAfterTrash;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip doorBellClip;
        [SerializeField, Min(0f)] private float doorBellDelay = 0.3f;

        private bool consumed;

        protected override IEnumerator Execute(StoryScenarioController controller)
        {
            if (trashTrigger == null)
            {
                Debug.LogWarning("Trash objective has no trash trigger.", this);
                yield break;
            }

            consumed = false;

            trashTrigger.PickupConsumed += OnPickupConsumed;

            yield return PlayDialogue(controller, linesBefore);

            while (!consumed)
                yield return null;

            if (trashTrigger != null)
                trashTrigger.PickupConsumed -= OnPickupConsumed;

            if (doorBellDelay > 0f)
                yield return new WaitForSeconds(doorBellDelay);

            PlayOneShot(doorBellClip);
            yield return PlayDialogue(controller, linesAfterTrash);
        }

        private IEnumerator PlayDialogue(StoryScenarioController controller, DialogueLine[] lines)
        {
            if (controller.DialogueRunner == null || lines == null || lines.Length == 0)
                yield break;

            controller.DialogueRunner.Play(this, lines);
            while (controller.DialogueRunner.IsPlaying)
                yield return null;
        }

        private void PlayOneShot(AudioClip clip)
        {
            if (audioSource != null && clip != null)
                audioSource.PlayOneShot(clip);
        }

        private void OnDisable()
        {
            if (trashTrigger != null)
                trashTrigger.PickupConsumed -= OnPickupConsumed;
        }

        private void OnPickupConsumed(IPickupInteractable pickup)
        {
            consumed = true;
        }
    }
}
