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

        private Rigidbody body;
        private Transform previousParent;
        private bool previousKinematic;
        private Camera inspectingCamera;
        private bool held;
        private bool rotatingInspect;

        public override string Prompt => takePrompt;
        public bool KeepsInteractionFocus => held;
        public bool IsRotatingInspect => rotatingInspect;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
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
                }

                var mouseX = Input.GetAxis("Mouse X") * rotateSensitivity;
                var mouseY = Input.GetAxis("Mouse Y") * rotateSensitivity;
                transform.Rotate(Vector3.up, -mouseX, Space.World);
                transform.Rotate(Vector3.right, mouseY, Space.World);
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
                PickUp(context);

            base.Interact(context);
        }

        private void PickUp(InteractionContext context)
        {
            var anchor = context.Camera != null ? context.Camera.transform : context.HoldPoint;
            if (anchor == null)
                return;

            previousParent = transform.parent;
            previousKinematic = body.isKinematic;
            inspectingCamera = context.Camera;

            transform.SetParent(anchor);
            ApplyHoldPose();
            body.isKinematic = true;
            held = true;
        }

        private void Drop()
        {
            ApplyInspectPose();
            transform.SetParent(previousParent);
            body.isKinematic = previousKinematic;
            ClearHeldState();
        }

        private void Throw()
        {
            transform.SetParent(previousParent);
            body.isKinematic = false;

            var direction = inspectingCamera != null ? inspectingCamera.transform.forward : transform.forward;
            body.AddForce(direction.normalized * throwForce, ForceMode.VelocityChange);
            ClearHeldState();
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

        private void ClearHeldState()
        {
            held = false;
            rotatingInspect = false;
            inspectingCamera = null;
        }
    }
}
