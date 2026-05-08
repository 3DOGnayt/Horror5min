using System;
using UnityEngine;

namespace HorrorCafe.Interaction
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class InspectablePickupObject : InteractableBase, IInteractionFocusLock, IPickupInteractable
    {
        [SerializeField] private string takePrompt;
        [SerializeField] private Vector3 holdLocalPosition = Vector3.zero;
        [SerializeField] private Vector3 holdLocalEuler = Vector3.zero;
        [SerializeField] private Vector3 inspectLocalPosition = new Vector3(0f, 0f, 0.55f);
        [SerializeField] private Vector3 inspectLocalEuler = Vector3.zero;
        [SerializeField] private float rotateSensitivity = 5f;
        [SerializeField] private float throwForce = 3.5f;
        [SerializeField] private bool rotateAroundVisualCenter = true;

        private Rigidbody body;
        private Transform previousParent;
        private Camera inspectingCamera;
        private bool held;
        private bool rotatingInspect;
        private Vector3 inspectRotationCenter;

        public event Action<InspectablePickupObject, PickupReleaseMode> Released;
        public event Action<InspectablePickupObject> PickedUp;

        public override string Prompt => takePrompt;
        public bool KeepsInteractionFocus => held;
        public bool IsRotatingInspect => rotatingInspect;
        public bool IsHeld => held;

        private void Awake()
        {
            EnsureBody();
        }

        private void Update()
        {
            if (!held)
                return;

            if (Input.GetMouseButtonDown(1))
            {
                Throw();
                return;
            }

            if (Input.GetKey(KeyCode.R))
            {
                if (!rotatingInspect)
                {
                    rotatingInspect = true;
                    ApplyInspectPose();
                    inspectRotationCenter = GetVisualCenter();
                }

                var mouseX = Input.GetAxis("Mouse X") * rotateSensitivity;
                var mouseY = Input.GetAxis("Mouse Y") * rotateSensitivity;
                RotateInspect(Vector3.up, -mouseX);
                RotateInspect(Vector3.right, mouseY);
                return;
            }

            if (rotatingInspect)
            {
                rotatingInspect = false;
                ApplyHoldPose();
            }
        }

        public override void Interact(InteractionContext context)
        {
            if (held)
                Drop();
            else
                TryPickUp(context);

            base.Interact(context);
        }

        public bool TryPickUp(InteractionContext context)
        {
            if (held)
                return false;

            return PickUp(context);
        }

        private bool PickUp(InteractionContext context)
        {
            EnsureBody();

            var anchor = context.Camera != null ? context.Camera.transform : context.HoldPoint;
            if (anchor == null || body == null)
                return false;

            previousParent = transform.parent;
            inspectingCamera = context.Camera;

            transform.SetParent(anchor);
            ApplyHoldPose();
            body.isKinematic = true;
            held = true;
            PickedUp?.Invoke(this);
            return true;
        }

        private void EnsureBody()
        {
            if (body == null)
                body = GetComponent<Rigidbody>();
        }

        private void Drop()
        {
            transform.SetParent(previousParent);
            body.isKinematic = false;
            ClearHeldState();
            Released?.Invoke(this, PickupReleaseMode.Drop);
        }

        private void Throw()
        {
            transform.SetParent(previousParent);
            body.isKinematic = false;

            var direction = inspectingCamera != null ? inspectingCamera.transform.forward : transform.forward;
            body.AddForce(direction.normalized * throwForce, ForceMode.VelocityChange);
            ClearHeldState();
            Released?.Invoke(this, PickupReleaseMode.Throw);
        }

        public void SnapTo(Transform target, Transform parent, bool canInteract, Vector3 localEulerOffset = default)
        {
            if (target == null)
                return;

            EnsureBody();
            transform.SetParent(parent, true);
            transform.SetPositionAndRotation(target.position, target.rotation * Quaternion.Euler(localEulerOffset));

            if (body != null)
            {
                body.velocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.isKinematic = true;
            }

            ClearHeldState();
            SetCanInteract(canInteract);
            enabled = canInteract;
        }

        public void SetCollidersEnabled(bool value)
        {
            var colliders = GetComponentsInChildren<Collider>(true);
            foreach (var itemCollider in colliders)
            {
                if (itemCollider != null)
                    itemCollider.enabled = value;
            }
        }

        public void SetPrompt(string value)
        {
            takePrompt = value;
        }

        private void ApplyHoldPose()
        {
            transform.localPosition = holdLocalPosition;
            transform.localRotation = Quaternion.Euler(holdLocalEuler);
        }

        private void ApplyInspectPose()
        {
            transform.localPosition = inspectLocalPosition;
            transform.localRotation = Quaternion.Euler(inspectLocalEuler);
        }

        private void RotateInspect(Vector3 axis, float angle)
        {
            if (rotateAroundVisualCenter)
                transform.RotateAround(inspectRotationCenter, axis, angle);
            else
                transform.Rotate(axis, angle, Space.World);
        }

        private Vector3 GetVisualCenter()
        {
            var renderers = GetComponentsInChildren<Renderer>(true);
            var hasBounds = false;
            var bounds = new Bounds(transform.position, Vector3.zero);

            foreach (var itemRenderer in renderers)
            {
                if (itemRenderer == null)
                    continue;

                if (hasBounds)
                    bounds.Encapsulate(itemRenderer.bounds);
                else
                {
                    bounds = itemRenderer.bounds;
                    hasBounds = true;
                }
            }

            return hasBounds ? bounds.center : transform.position;
        }

        private void ClearHeldState()
        {
            held = false;
            rotatingInspect = false;
            inspectingCamera = null;
        }
    }
}
