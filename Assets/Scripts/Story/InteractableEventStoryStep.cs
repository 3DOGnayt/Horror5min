using System.Collections;
using HorrorCafe.Interaction;
using UnityEngine;

namespace HorrorCafe.Story
{
    public sealed class InteractableEventStoryStep : StoryStep
    {
        [SerializeField] private InteractableBase interactable;
        [SerializeField] private DialogueLine[] linesBefore;
        [SerializeField] private DialogueLine[] linesAfter;

        private bool interacted;

        protected override IEnumerator Execute(StoryScenarioController controller)
        {
            if (interactable == null)
            {
                Debug.LogWarning("Interactable event step has no interactable.", this);
                yield break;
            }

            interacted = false;

            var coffeeGive = interactable as NpcCoffeeGiveInteractable;

            // Пока NPC говорит — кофе не принимается.
            coffeeGive?.SetCanAcceptCoffee(false);

            interactable.Interacted += OnInteracted;

            yield return PlayDialogue(controller, linesBefore);

            // Теперь NPC договорил, можно принимать кофе.
            coffeeGive?.SetCanAcceptCoffee(true);

            while (!interacted)
                yield return null;

            // После успешной отдачи снова выключаем приём.
            coffeeGive?.SetCanAcceptCoffee(false);

            interactable.Interacted -= OnInteracted;

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

        private void OnInteracted(InteractableBase source, InteractionContext context)
        {
            interacted = true;
        }

        private void OnDisable()
        {
            if (interactable != null)
                interactable.Interacted -= OnInteracted;

            if (interactable is NpcCoffeeGiveInteractable coffeeGive)
                coffeeGive.SetCanAcceptCoffee(false);
        }
    }
}