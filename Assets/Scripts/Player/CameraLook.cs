using UnityEngine;

namespace HorrorCafe.Player
{
    public sealed class CameraLook : MonoBehaviour
    {
        [SerializeField] private Transform playerBody;
        [SerializeField] private float sensitivity = 2.2f;
        [SerializeField] private float minPitch = -78f;
        [SerializeField] private float maxPitch = 78f;
        [SerializeField] private bool lockCursorOnStart = true;
        
        private float pitch;
        private bool lookEnabled = true;

        public bool LookEnabled
        {
            get => lookEnabled;
            set => lookEnabled = value;
        }

        private void Start()
        {
            if (!lockCursorOnStart) 
                return;
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (!lookEnabled)
                return;
            
            var mouseX = Input.GetAxis("Mouse X") * sensitivity;
            var mouseY = Input.GetAxis("Mouse Y") * sensitivity;
            
            pitch = Mathf.Clamp(pitch - mouseY, minPitch, maxPitch);
            transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            
            if (playerBody != null) 
                playerBody.Rotate(Vector3.up * mouseX);
        }
    }
}