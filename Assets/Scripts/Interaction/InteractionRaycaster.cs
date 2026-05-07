using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HorrorCafe.Interaction
{
    public sealed class InteractionRaycaster : MonoBehaviour
    {
        [SerializeField] private Camera sourceCamera;
        [SerializeField] private Transform holdPoint;
        [SerializeField] private TMP_Text interactionText;
        [SerializeField] private GameObject idleIndicator;
        [SerializeField] private GameObject focusedIndicator;
        [SerializeField] private Image focusedIndicatorImage;
        [SerializeField] private Color focusedIndicatorColor = Color.white;
        [SerializeField] private Color blockedIndicatorColor = Color.red;
        [SerializeField] private GameObject movedHints;
        [SerializeField] private float range = 2.2f;
        [SerializeField] private LayerMask interactionMask = ~0;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        private IInteractable focused;
        private IInteractionFocusLock heldFocus;

        private void Awake()
        {
            SetInteractionText(string.Empty, false);
            if (movedHints != null)
                movedHints.SetActive(false);
        }

        private void Reset()
        {
            sourceCamera = GetComponentInChildren<Camera>();
        }

        private void Update()
        {
            UpdateFocus();
            if (!Input.GetKeyDown(interactKey))
                return;

            var heldInteractable = heldFocus as IInteractable;
            var canUseHeldItem = heldInteractable != null && heldInteractable.CanInteract;
            var blocksPickup = IsBlockedPickupFocus(focused);
            var canUseWorldFocus = focused != null && focused.CanInteract && !ReferenceEquals(focused, heldInteractable) && !blocksPickup;

            if (canUseWorldFocus)
            {
                focused.Interact(new InteractionContext(gameObject, sourceCamera, holdPoint));
                return;
            }

            if (canUseHeldItem)
            {
                heldInteractable.Interact(new InteractionContext(gameObject, sourceCamera, holdPoint));
                return;
            }

            if (focused != null && focused.CanInteract)
            {
                focused.Interact(new InteractionContext(gameObject, sourceCamera, holdPoint));
            }
        }

        private void UpdateFocus()
        {
            UpdateHeldFocus();

            var next = FindInteractable();
            if (!ReferenceEquals(focused, next))
            {
                focused?.Unfocus();
                focused = next;
                focused?.Focus();
            }
            ApplyFocusUi(focused);
        }

        private void ApplyFocusUi(IInteractable interactable)
        {
            var hideWorldFocus = heldFocus != null && heldFocus.IsRotatingInspect;
            var hasFocus = !hideWorldFocus && interactable != null && interactable.CanInteract;
            var blockedPickup = hasFocus && IsBlockedPickupFocus(interactable);
            var prompt = hasFocus && !blockedPickup ? interactable.Prompt : string.Empty;

            SetInteractionText(prompt, !string.IsNullOrWhiteSpace(prompt));

            if (idleIndicator != null)
                idleIndicator.SetActive(!hideWorldFocus);

            if (focusedIndicator != null)
                focusedIndicator.SetActive(hasFocus);

            if (focusedIndicatorImage != null)
                focusedIndicatorImage.color = blockedPickup ? blockedIndicatorColor : focusedIndicatorColor;

            if (movedHints != null)
                movedHints.SetActive(heldFocus != null);
        }

        private void SetInteractionText(string text, bool visible)
        {
            if (interactionText == null)
                return;

            interactionText.text = visible ? text : string.Empty;
            interactionText.gameObject.SetActive(visible);
        }

        private IInteractable FindInteractable()
        {
            if (sourceCamera == null)
            {
                return null;
            }
            var ray = new Ray(sourceCamera.transform.position, sourceCamera.transform.forward);
            if (!Physics.Raycast(ray, out var hit, range, interactionMask, QueryTriggerInteraction.Collide))
            {
                return null;
            }
            return hit.collider.GetComponentInParent<IInteractable>();
        }

        private void UpdateHeldFocus()
        {
            var currentFocusLock = focused as IInteractionFocusLock;
            if (currentFocusLock != null && currentFocusLock.KeepsInteractionFocus)
            {
                heldFocus = currentFocusLock;
                return;
            }

            if (heldFocus != null && !heldFocus.KeepsInteractionFocus)
                heldFocus = null;
        }

        private bool IsBlockedPickupFocus(IInteractable interactable)
        {
            if (heldFocus == null || interactable == null)
                return false;

            if (ReferenceEquals(interactable, heldFocus))
                return false;

            return interactable is IPickupInteractable;
        }
    }
}
