namespace HorrorCafe.Interaction
{
    public interface IInteractable
    {
        string Prompt { get; }
        bool CanInteract { get; }
        void Interact(InteractionContext context);
        void Focus();
        void Unfocus();
    }
}
