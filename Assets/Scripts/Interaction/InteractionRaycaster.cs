using System.Collections.Generic;
using TMPro;
using HorrorCafe.Player;
using HorrorCafe.UI;
using UnityEngine;
using UnityEngine.UI;

namespace HorrorCafe.Interaction
{
    [DefaultExecutionOrder(-100)]
    public sealed class InteractionRaycaster : MonoBehaviour
    {
        [SerializeField] private Camera sourceCamera;
        [SerializeField] private CameraLook cameraLook;
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
        private readonly Dictionary<Collider, IInteractable> interactableCache = new Dictionary<Collider, IInteractable>();

        private void Awake()
        {
            ResolveCameraLook();

            SetInteractionText(string.Empty, false);
            if (movedHints != null)
                movedHints.SetActive(false);
        }

        private void Reset()
        {
            sourceCamera = GetComponentInChildren<Camera>();
            ResolveCameraLook();
        }

        private void OnDisable()
        {
            SetCameraLookBlocked(false);
        }

        private void Update()
        {
            UpdateFocus();
            UpdateCameraLookLock();
            if (!Input.GetKeyDown(interactKey))
                return;

            var heldInteractable = heldFocus as IInteractable;
            var canUseHeldItem = heldInteractable != null && heldInteractable.CanInteract;
            var blocksPickup = IsBlockedPickupFocus(focused);
            var canUseWorldFocus = focused != null && focused.CanInteract && !ReferenceEquals(focused, heldInteractable) && !blocksPickup;

            if (canUseWorldFocus)
            {
                focused.Interact(CreateInteractionContext());
                return;
            }

            if (canUseHeldItem)
            {
                heldInteractable.Interact(CreateInteractionContext());
                return;
            }

            if (focused != null && focused.CanInteract)
            {
                focused.Interact(CreateInteractionContext());
            }
        }

        private InteractionContext CreateInteractionContext()
        {
            return new InteractionContext(gameObject, sourceCamera, holdPoint, SetHeldFocus);
        }

        private void SetHeldFocus(IInteractionFocusLock focusLock)
        {
            heldFocus = focusLock != null && focusLock.KeepsInteractionFocus ? focusLock : null;
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
            var hidePrompt = IsPlayerMessageShowing();
            var prompt = hasFocus && !blockedPickup && !hidePrompt ? interactable.Prompt : string.Empty;

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

        private bool IsPlayerMessageShowing()
        {
            var messageService = PlayerMessageService.Instance;
            return messageService != null && messageService.IsShowing;
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
            var hits = Physics.RaycastAll(ray, range, interactionMask, QueryTriggerInteraction.Collide);
            var closestDistance = float.PositiveInfinity;

            IInteractable closestInteractable = null;

            foreach (var hit in hits)
            {
                if (IsHeldFocusCollider(hit.collider) || hit.distance >= closestDistance)
                    continue;

                var interactable = ResolveInteractable(hit.collider);
                if (interactable == null)
                    continue;

                closestDistance = hit.distance;
                closestInteractable = interactable;
            }

            return closestInteractable;
        }

        private IInteractable ResolveInteractable(Collider collider)
        {
            if (collider == null)
                return null;

            if (!interactableCache.TryGetValue(collider, out var interactable))
            {
                interactable = collider.GetComponentInParent<IInteractable>();
                interactableCache.Add(collider, interactable);
            }

            return interactable;
        }

        private bool IsHeldFocusCollider(Collider collider)
        {
            if (collider == null || heldFocus is not Component heldComponent)
                return false;

            return collider.transform.IsChildOf(heldComponent.transform);
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

        private void UpdateCameraLookLock()
        {
            var blocksLook = heldFocus != null && (heldFocus.IsRotatingInspect || Input.GetKey(KeyCode.R));
            SetCameraLookBlocked(blocksLook);
        }

        private void SetCameraLookBlocked(bool blocked)
        {
            if (cameraLook != null)
                cameraLook.InteractionLookBlocked = blocked;
        }

        private void ResolveCameraLook()
        {
            if (cameraLook != null)
                return;

            if (sourceCamera != null)
                cameraLook = sourceCamera.GetComponentInParent<CameraLook>();

            if (cameraLook == null)
                cameraLook = GetComponentInChildren<CameraLook>();
        }

        private bool IsBlockedPickupFocus(IInteractable interactable)
        {
            if (heldFocus == null || interactable == null)
                return false;

            if (ReferenceEquals(interactable, heldFocus))
                return false;

            return interactable is IPickupInteractable || interactable is IRequiresEmptyHands;
        }
    }
}
