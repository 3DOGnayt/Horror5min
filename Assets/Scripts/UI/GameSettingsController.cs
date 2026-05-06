using HorrorCafe.Player;
using UnityEngine;

namespace HorrorCafe.UI
{
    public sealed class GameSettingsController : MonoBehaviour
    {
        [SerializeField] private SettingsController settingsController;
        [SerializeField] private HorrorPlayerController playerController;
        [SerializeField] private CameraLook cameraLook;

        private float previousTimeScale = 1f;
        private CursorLockMode previousCursorLockMode;
        private bool previousCursorVisible;

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
            previousTimeScale = Time.timeScale;
            previousCursorLockMode = Cursor.lockState;
            previousCursorVisible = Cursor.visible;

            Time.timeScale = 0f;
            SetGameInputEnabled(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            settingsController.OpenSettings();
        }

        private void ResumeGame()
        {
            Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;
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
        }
    }
}
