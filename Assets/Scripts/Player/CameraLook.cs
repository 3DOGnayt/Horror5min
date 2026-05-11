using UnityEngine;

namespace HorrorCafe.Player
{
    public sealed class CameraLook : MonoBehaviour
    {
        [SerializeField] private Transform playerBody;
        [SerializeField] private Camera playerCamera;
        [SerializeField, Min(0.01f)] private float nearClipPlane = 0.03f;
        [SerializeField] private float sensitivity = 2.2f;
        [SerializeField] private float minPitch = -78f;
        [SerializeField] private float maxPitch = 78f;
        [SerializeField] private bool lockCursorOnStart = true;
        
        private float pitch;
        private bool lookEnabled = true;
        private bool interactionLookBlocked;

        public bool LookEnabled
        {
            get => lookEnabled;
            set => lookEnabled = value;
        }

        public bool InteractionLookBlocked
        {
            get => interactionLookBlocked;
            set => interactionLookBlocked = value;
        }

        public void SetPitch(float value)
        {
            pitch = Mathf.Clamp(value, minPitch, maxPitch);
            transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        public void LookAt(Vector3 worldPosition)
        {
            var origin = playerCamera != null ? playerCamera.transform.position : transform.position;
            SetLookDirection(worldPosition - origin);
        }

        public void SetWorldLookRotation(Quaternion worldRotation)
        {
            SetLookDirection(worldRotation * Vector3.forward);
        }

        private void Awake() => ApplyCameraClipPlane();

        private void OnValidate() => ApplyCameraClipPlane();

        private void Start()
        {
            if (!lockCursorOnStart) 
                return;
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (!lookEnabled || interactionLookBlocked)
                return;
            
            var mouseX = Input.GetAxis("Mouse X") * sensitivity;
            var mouseY = Input.GetAxis("Mouse Y") * sensitivity;
            
            pitch = Mathf.Clamp(pitch - mouseY, minPitch, maxPitch);
            transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            
            if (playerBody != null) 
                playerBody.Rotate(Vector3.up * mouseX);
        }

        private void ApplyCameraClipPlane()
        {
            if (playerCamera == null) 
                playerCamera = GetComponentInChildren<Camera>();

            if (playerCamera != null) 
                playerCamera.nearClipPlane = nearClipPlane;
        }

        private void SetLookDirection(Vector3 worldDirection)
        {
            if (worldDirection.sqrMagnitude <= 0.0001f)
                return;

            worldDirection.Normalize();

            if (playerBody != null)
            {
                var flatDirection = new Vector3(worldDirection.x, 0f, worldDirection.z);
                if (flatDirection.sqrMagnitude > 0.0001f)
                    playerBody.rotation = Quaternion.LookRotation(flatDirection.normalized, Vector3.up);

                var localDirection = playerBody.InverseTransformDirection(worldDirection);
                var horizontalMagnitude = new Vector2(localDirection.x, localDirection.z).magnitude;
                SetPitch(-Mathf.Atan2(localDirection.y, horizontalMagnitude) * Mathf.Rad2Deg);
                return;
            }

            transform.rotation = Quaternion.LookRotation(worldDirection, Vector3.up);
            pitch = NormalizePitch(transform.localEulerAngles.x);
        }

        private float NormalizePitch(float value)
        {
            if (value > 180f)
                value -= 360f;

            return Mathf.Clamp(value, minPitch, maxPitch);
        }
    }
}
