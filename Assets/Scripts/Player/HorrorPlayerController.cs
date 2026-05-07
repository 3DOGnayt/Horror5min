using UnityEngine;

namespace HorrorCafe.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class HorrorPlayerController : MonoBehaviour
    {
        [SerializeField] private float walkSpeed = 2.6f;
        [SerializeField] private float runSpeed = 4.4f;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private bool canRun = true;
        [SerializeField] private bool lockYPosition = true;

        private bool controlsEnabled = true;
        private float lockedY;

        public bool ControlsEnabled
        {
            get => controlsEnabled;
            set => controlsEnabled = value;
        }

        private void Awake()
        {
            lockedY = transform.position.y;
        }

        private void Update()
        {
            if (!controlsEnabled)
            {
                return;
            }
            var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            input = Vector2.ClampMagnitude(input, 1f);

            var wantsRun = canRun && Input.GetKey(KeyCode.LeftShift);
            var speed = wantsRun ? runSpeed : walkSpeed;
            var move = transform.right * input.x + transform.forward * input.y;

            characterController.Move(move * (speed * Time.deltaTime));
            ApplyYLock();
        }

        private void ApplyYLock()
        {
            if (!lockYPosition)
                return;

            var position = transform.position;
            position.y = lockedY;
            transform.position = position;
        }
    }
}
