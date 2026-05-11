using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HorrorCafe.UI
{
    public sealed class SettingsMenuView : MonoBehaviour
    {
        [SerializeField] private SettingsController controller;
        
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Toggle muteToggle;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button returnToMenu;

        private bool binding;

        private void Awake()
        {
            if (controller == null) 
                controller = GetComponent<SettingsController>();
        }

        private void OnEnable()
        {
            Bind();
            Refresh();
        }

        private void OnDisable()
        {
            Unbind();
        }

        private void Bind()
        {
            Unbind();
            masterVolumeSlider?.onValueChanged.AddListener(OnMasterVolumeChanged);
            muteToggle?.onValueChanged.AddListener(OnMutedChanged);
            closeButton?.onClick.AddListener(CloseSettings);
            returnToMenu?.onClick.AddListener(ReturnToMenu);
        }

        private void ReturnToMenu()
        {
            SceneManager.LoadScene(0);
        }

        private void Unbind()
        {
            masterVolumeSlider?.onValueChanged.RemoveListener(OnMasterVolumeChanged);
            muteToggle?.onValueChanged.RemoveListener(OnMutedChanged);
            closeButton?.onClick.RemoveListener(CloseSettings);
            returnToMenu?.onClick.RemoveListener(ReturnToMenu);
        }

        private void Refresh()
        {
            if (controller == null)
                return;

            binding = true;
            masterVolumeSlider?.SetValueWithoutNotify(controller.MasterVolume);
            muteToggle?.SetIsOnWithoutNotify(controller.Muted);
            binding = false;
        }

        private void OnMasterVolumeChanged(float value)
        {
            if (!binding) 
                controller?.SetMasterVolume(value);
        }

        private void OnMutedChanged(bool value)
        {
            if (!binding) 
                controller?.SetMuted(value);
        }

        private void CloseSettings()
        {
            controller?.CloseSettings();
        }
    }
}