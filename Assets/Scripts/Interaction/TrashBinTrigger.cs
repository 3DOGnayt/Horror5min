using UnityEngine;

namespace HorrorCafe.Interaction
{
    [RequireComponent(typeof(Collider))]
    public sealed class TrashBinTrigger : MonoBehaviour
    {
        [SerializeField] private TrashBinInteractable trashBin;

        private void Reset()
        {
            trashBin = GetComponentInParent<TrashBinInteractable>();
            SetColliderTrigger();
        }

        private void OnValidate()
        {
            SetColliderTrigger();
        }

        private void OnTriggerEnter(Collider other)
        {
            TryConsume(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryConsume(other);
        }

        private void TryConsume(Collider other)
        {
            if (trashBin == null || !trashBin.IsOpen || other == null)
                return;

            var pickup = other.GetComponentInParent<IPickupInteractable>();
            if (pickup == null || IsHeld(pickup))
                return;

            if (pickup is Component pickupComponent)
                pickupComponent.gameObject.SetActive(false);
        }

        private static bool IsHeld(IPickupInteractable pickup)
        {
            return pickup is IInteractionFocusLock focusLock && focusLock.KeepsInteractionFocus;
        }

        private void SetColliderTrigger()
        {
            var triggerCollider = GetComponent<Collider>();
            if (triggerCollider != null)
                triggerCollider.isTrigger = true;
        }
    }
}
