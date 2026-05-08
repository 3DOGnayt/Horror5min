using System.Collections.Generic;
using UnityEngine;

namespace HorrorCafe.Interaction
{
    [RequireComponent(typeof(Collider))]
    public sealed class CoffeeMachinePlacementTrigger : MonoBehaviour
    {
        [SerializeField] private CoffeeMachineInteractable coffeeMachine;
        [SerializeField] private CoffeePickupKind acceptedKind;

        private readonly HashSet<InspectablePickupObject> trackedPickups = new HashSet<InspectablePickupObject>();

        private void Reset()
        {
            coffeeMachine = GetComponentInParent<CoffeeMachineInteractable>();
            SetColliderTrigger();
        }

        private void OnValidate()
        {
            SetColliderTrigger();
        }

        private void OnDisable()
        {
            foreach (var pickup in trackedPickups)
                Unsubscribe(pickup);

            trackedPickups.Clear();
        }

        private void OnTriggerEnter(Collider other)
        {
            Track(other);
        }

        private void OnTriggerStay(Collider other)
        {
            var pickup = Track(other);
            if (pickup != null && !pickup.IsHeld)
                TryPlace(pickup);
        }

        private void OnTriggerExit(Collider other)
        {
            var pickup = other != null ? other.GetComponentInParent<InspectablePickupObject>() : null;
            if (pickup == null || !trackedPickups.Remove(pickup))
                return;

            Unsubscribe(pickup);
        }

        private InspectablePickupObject Track(Collider other)
        {
            var pickup = other != null ? other.GetComponentInParent<InspectablePickupObject>() : null;
            if (pickup == null || trackedPickups.Contains(pickup))
                return pickup;

            trackedPickups.Add(pickup);
            pickup.Released += OnPickupReleased;
            return pickup;
        }

        private void OnPickupReleased(InspectablePickupObject pickup, PickupReleaseMode releaseMode)
        {
            if (pickup != null && trackedPickups.Contains(pickup))
                TryPlace(pickup);
        }

        private void TryPlace(InspectablePickupObject pickup)
        {
            if (coffeeMachine == null || pickup == null)
                return;

            if (coffeeMachine.TryPlaceReleasedPickup(pickup, acceptedKind))
            {
                trackedPickups.Remove(pickup);
                Unsubscribe(pickup);
            }
        }

        private void Unsubscribe(InspectablePickupObject pickup)
        {
            if (pickup != null)
                pickup.Released -= OnPickupReleased;
        }

        private void SetColliderTrigger()
        {
            var triggerCollider = GetComponent<Collider>();
            if (triggerCollider != null)
                triggerCollider.isTrigger = true;
        }
    }
}
