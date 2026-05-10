using UnityEngine;

namespace HorrorCafe.Interaction
{
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        [SerializeField] private string prompt = "[E] Interact";
        [SerializeField] private bool canInteract = true;
        public virtual string Prompt => prompt;
        public virtual bool CanInteract => canInteract && isActiveAndEnabled;

        public event System.Action<InteractableBase, InteractionContext> Interacted;

        public virtual void Interact(InteractionContext context)
        {
            Interacted?.Invoke(this, context);
        }

        public virtual void Focus()
        {
        }

        public virtual void Unfocus()
        {
        }

        public void SetCanInteract(bool value)
        {
            canInteract = value;
        }
    }
}
