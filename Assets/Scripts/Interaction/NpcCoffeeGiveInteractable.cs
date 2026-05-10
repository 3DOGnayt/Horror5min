using UnityEngine;

namespace HorrorCafe.Interaction
{
    public sealed class NpcCoffeeGiveInteractable : InteractableBase, IHeldFocusPreview
    {
        [SerializeField] private string giveCoffeePrompt = "[E] Отдать кофе";
        [SerializeField] private bool disableCoffeeOnGive = true;

        private InspectablePickupObject previewHeldPickup;
        private bool coffeeGiven;

        public override string Prompt => giveCoffeePrompt;
        public override bool CanInteract => base.CanInteract && IsReadyCoffee(previewHeldPickup);

        public void SetHeldFocusPreview(IInteractionFocusLock heldFocus)
        {
            previewHeldPickup = heldFocus as InspectablePickupObject;
        }

        public override void Interact(InteractionContext context)
        {
            var coffee = context.HeldPickup;
            if (!IsReadyCoffee(coffee))
                return;

            context.SetHeldFocus(null);
            AcceptCoffee(coffee, context);
        }

        private void OnTriggerEnter(Collider other)
        {
            TryAcceptReleasedCoffee(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryAcceptReleasedCoffee(other);
        }

        private void TryAcceptReleasedCoffee(Collider other)
        {
            if (coffeeGiven || other == null)
                return;

            var coffee = other.GetComponentInParent<InspectablePickupObject>();
            if (!IsReadyCoffee(coffee) || coffee.IsHeld)
                return;

            AcceptCoffee(coffee, default);
        }

        private void AcceptCoffee(InspectablePickupObject coffee, InteractionContext context)
        {
            if (coffeeGiven || coffee == null)
                return;

            coffeeGiven = true;

            if (disableCoffeeOnGive)
                coffee.gameObject.SetActive(false);

            base.Interact(context);
        }

        private static bool IsReadyCoffee(InspectablePickupObject pickup)
        {
            if (pickup == null)
                return false;

            var tag = pickup.GetComponentInChildren<CoffeePickupTag>(true);
            return tag != null && tag.Kind == CoffeePickupKind.ReadyCoffee;
        }
    }
}
