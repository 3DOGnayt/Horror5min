using System.Collections;
using HorrorCafe.Interaction;
using UnityEngine;

namespace HorrorCafe.Story
{
    public sealed class CoffeeObjectiveStoryStep : StoryStep
    {
        private enum WaitFor
        {
            CupPlaced,
            BrewingStarted,
            BrewingFinishedNeedsLid,
            LidPlaced,
            CoffeeReady,
            ReadyCoffeePickedUp
        }

        [SerializeField] private CoffeeMachineInteractable coffeeMachine;
        [SerializeField] private WaitFor waitFor;
        [SerializeField] private DialogueLine[] linesBefore;
        [SerializeField] private DialogueLine[] linesAfter;

        private bool reached;

        protected override IEnumerator Execute(StoryScenarioController controller)
        {
            if (coffeeMachine == null)
            {
                Debug.LogWarning("Coffee objective has no coffee machine.", this);
                yield break;
            }

            reached = false;
            Subscribe();

            yield return PlayDialogue(controller, linesBefore);

            while (!reached)
                yield return null;

            Unsubscribe();
            yield return PlayDialogue(controller, linesAfter);
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
            if (coffeeMachine == null)
                return;

            coffeeMachine.CupPlaced += OnCupPlaced;
            coffeeMachine.BrewingStarted += OnBrewingStarted;
            coffeeMachine.BrewingFinishedNeedsLid += OnBrewingFinishedNeedsLid;
            coffeeMachine.LidPlaced += OnLidPlaced;
            coffeeMachine.CoffeeReady += OnCoffeeReady;
            coffeeMachine.ReadyCoffeePickedUp += OnReadyCoffeePickedUp;
        }

        private void Unsubscribe()
        {
            if (coffeeMachine == null)
                return;

            coffeeMachine.CupPlaced -= OnCupPlaced;
            coffeeMachine.BrewingStarted -= OnBrewingStarted;
            coffeeMachine.BrewingFinishedNeedsLid -= OnBrewingFinishedNeedsLid;
            coffeeMachine.LidPlaced -= OnLidPlaced;
            coffeeMachine.CoffeeReady -= OnCoffeeReady;
            coffeeMachine.ReadyCoffeePickedUp -= OnReadyCoffeePickedUp;
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnCupPlaced(InspectablePickupObject pickup)
        {
            if (waitFor == WaitFor.CupPlaced)
                reached = true;
        }

        private void OnBrewingStarted()
        {
            if (waitFor == WaitFor.BrewingStarted)
                reached = true;
        }

        private void OnBrewingFinishedNeedsLid()
        {
            if (waitFor == WaitFor.BrewingFinishedNeedsLid)
                reached = true;
        }

        private void OnLidPlaced(InspectablePickupObject pickup)
        {
            if (waitFor == WaitFor.LidPlaced)
                reached = true;
        }

        private void OnCoffeeReady(InspectablePickupObject pickup)
        {
            if (waitFor == WaitFor.CoffeeReady)
                reached = true;
        }

        private void OnReadyCoffeePickedUp(InspectablePickupObject pickup)
        {
            if (waitFor == WaitFor.ReadyCoffeePickedUp)
                reached = true;
        }
    }
}
