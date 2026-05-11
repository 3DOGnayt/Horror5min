using System.Collections.Generic;
using UnityEngine;

namespace HorrorCafe.Interaction
{
    public sealed class NpcCoffeeGiveInteractable : InteractableBase, IHeldFocusPreview
    {
        [SerializeField] private string giveCoffeePrompt = "[E] Отдать кофе";
        [SerializeField] private bool disableCoffeeOnGive = true;

        private InspectablePickupObject previewHeldPickup;

        private bool coffeeGiven;
        private bool canAcceptCoffee;

        private readonly HashSet<InspectablePickupObject> ignoredUntilExit = new();

        public override string Prompt => giveCoffeePrompt;

        public override bool CanInteract =>
            canAcceptCoffee &&
            base.CanInteract &&
            IsReadyCoffee(previewHeldPickup);

        public void SetCanAcceptCoffee(bool value)
        {
            canAcceptCoffee = value;
        }

        public void SetHeldFocusPreview(IInteractionFocusLock heldFocus)
        {
            previewHeldPickup = heldFocus as InspectablePickupObject;
        }

        public override void Interact(InteractionContext context)
        {
            if (!canAcceptCoffee)
                return;

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

        private void OnTriggerExit(Collider other)
        {
            var coffee = other.GetComponentInParent<InspectablePickupObject>();
            if (coffee != null)
                ignoredUntilExit.Remove(coffee);
        }

        private void TryAcceptReleasedCoffee(Collider other)
        {
            if (coffeeGiven || other == null)
                return;

            var coffee = other.GetComponentInParent<InspectablePickupObject>();
            if (!IsReadyCoffee(coffee) || coffee.IsHeld)
                return;

            // NPC ещё говорит.
            // Запоминаем этот кофе как "кинули слишком рано".
            if (!canAcceptCoffee)
            {
                ignoredUntilExit.Add(coffee);
                return;
            }

            // Этот кофе уже был внутри trigger до того,
            // как NPC закончил говорить. Не принимаем его.
            if (ignoredUntilExit.Contains(coffee))
                return;

            AcceptCoffee(coffee, default);
        }

        private void AcceptCoffee(InspectablePickupObject coffee, InteractionContext context)
        {
            if (!canAcceptCoffee || coffeeGiven || coffee == null)
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