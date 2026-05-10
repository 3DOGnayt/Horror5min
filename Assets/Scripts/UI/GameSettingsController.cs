using HorrorCafe.Interaction;
using HorrorCafe.Player;
using UnityEngine;

namespace HorrorCafe.UI
{
    public sealed class GameSettingsController : MonoBehaviour
    {
        [SerializeField] private SettingsController settingsController;
        [SerializeField] private HorrorPlayerController playerController;
        [SerializeField] private CameraLook cameraLook;
        [SerializeField] private InteractionRaycaster interactionRaycaster;

        private CursorLockMode previousCursorLockMode;
        private bool previousCursorVisible;
        private bool settingsOpenedByGameController;

        private void Awake()
        {
            if (interactionRaycaster == null)
                interactionRaycaster = FindObjectOfType<InteractionRaycaster>();
        }

        private void OnEnable()
        {
            if (settingsController != null)
                settingsController.SettingsClosed += ResumeGame;
        }

        private void OnDisable()
        {
            if (settingsController != null)
                settingsController.SettingsClosed -= ResumeGame;
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape) || settingsController == null)
                return;

            if (settingsController.IsOpen)
                settingsController.CloseSettings();
            else
                OpenSettings();
        }

        private void OpenSettings()
        {
            settingsOpenedByGameController = true;
            previousCursorLockMode = Cursor.lockState;
            previousCursorVisible = Cursor.visible;

            SetGameInputEnabled(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            settingsController.OpenSettings();
        }

        private void ResumeGame()
        {
            if (!settingsOpenedByGameController)
                return;

            settingsOpenedByGameController = false;
            SetGameInputEnabled(true);
            Cursor.lockState = previousCursorLockMode;
            Cursor.visible = previousCursorVisible;
        }

        private void SetGameInputEnabled(bool enabled)
        {
            if (playerController != null)
                playerController.ControlsEnabled = enabled;

            if (cameraLook != null)
                cameraLook.LookEnabled = enabled;

            if (interactionRaycaster != null)
                interactionRaycaster.enabled = enabled;
        }
    }
}
