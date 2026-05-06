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

        private bool controlsEnabled = true;

        public bool ControlsEnabled
        {
            get => controlsEnabled;
            set => controlsEnabled = value;
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
        }
    }
}
