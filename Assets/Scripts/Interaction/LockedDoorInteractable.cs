using HorrorCafe.UI;
using UnityEngine;

namespace HorrorCafe.Interaction
{
    public sealed class LockedDoorInteractable : InteractableBase
    {
        [SerializeField] private PlayerMessageService messageService;
        [SerializeField] private string lockedMessage;
        [SerializeField, Min(0.1f)] private float messageDuration = 2f;

        public override void Interact(InteractionContext context)
        {
            var service = messageService != null ? messageService : PlayerMessageService.Instance;
            if (service != null)
                service.Show(lockedMessage, messageDuration);
            else
                Debug.Log(lockedMessage, this);

            base.Interact(context);
        }
    }
}
