using System.Collections;
using HorrorCafe.Interaction;
using UnityEngine;

namespace HorrorCafe.Story
{
    public sealed class PickupObjectiveStoryStep : StoryStep
    {
        [SerializeField] private InspectablePickupObject[] acceptedPickups;
        [SerializeField] private DialogueLine[] linesBefore;
        [SerializeField] private DialogueLine[] linesAfterPickup;

        private bool pickedUp;

        protected override IEnumerator Execute(StoryScenarioController controller)
        {
            if (acceptedPickups == null || acceptedPickups.Length == 0)
            {
                Debug.LogWarning("Pickup objective has no accepted pickups.", this);
                yield break;
            }

            pickedUp = false;
            Subscribe();

            yield return PlayDialogue(controller, linesBefore);

            while (!pickedUp)
                yield return null;

            Unsubscribe();

            yield return PlayDialogue(controller, linesAfterPickup);
        }

        private IEnumerator PlayDialogue(StoryScenarioController controller, DialogueLine[] lines)
        {
            if (controller.DialogueRunner == null || lines == null || lines.Length == 0)
                yield break;

            controller.DialogueRunner.Play(this, lines);
            while (controller.DialogueRunner.IsPlaying)
                yield return null;
        }

        private void Subscribe()
        {
            if (acceptedPickups == null)
                return;

            foreach (var pickup in acceptedPickups)
            {
                if (pickup != null)
                    pickup.PickedUp += OnPickedUp;
            }
        }

        private void Unsubscribe()
        {
            if (acceptedPickups == null)
                return;

            foreach (var pickup in acceptedPickups)
            {
                if (pickup != null)
                    pickup.PickedUp -= OnPickedUp;
            }
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnPickedUp(InspectablePickupObject pickup)
        {
            pickedUp = true;
        }
    }
}
