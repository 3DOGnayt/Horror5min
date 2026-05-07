namespace HorrorCafe.Interaction
{
    public interface IInteractionFocusLock
    {
        bool KeepsInteractionFocus { get; }
        bool IsRotatingInspect { get; }
    }
}
