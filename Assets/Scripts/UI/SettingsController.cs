using System;
using HorrorCafe.Audio;
using UnityEngine;

namespace HorrorCafe.UI
{
    public sealed class SettingsController : MonoBehaviour
    {
        private const string VolumeKey = "HorrorCafe.Settings.MasterVolume";
        private const string MutedKey = "HorrorCafe.Settings.Muted";

        [SerializeField] private AudioSettingsController audioSettings;
        [SerializeField] private CanvasGroup canvasGroup;

        public event Action SettingsClosed;
        private bool isOpen;

        public bool IsOpen => isOpen;

        private AudioSettingsController AudioSettings
        {
            get
            {
                if (audioSettings != null)
                    return audioSettings;

                return AudioSettingsController.Instance;
            }
        }

        public float MasterVolume
        {
            get
            {
                if (AudioSettings != null)
                    return AudioSettings.GetVolume(AudioMixerChannel.Master);

                return PlayerPrefs.GetFloat(VolumeKey, 1f);
            }
        }

        public bool Muted
        {
            get
            {
                if (AudioSettings != null)
                    return AudioSettings.Muted;

                return PlayerPrefs.GetInt(MutedKey, 0) == 1;
            }
        }

        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            if (audioSettings == null) 
                ApplyPlaceholderAudio();

            SetVisible(false);
        }

        public void OpenSettings()
        {
            if (IsOpen)
                return;

            SetVisible(true);
        }

        public void CloseSettings()
        {
            if (!IsOpen)
                return;

            SetVisible(false);
            SettingsClosed?.Invoke();
        }

        private void SetVisible(bool visible)
        {
            isOpen = visible;
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }

        public void SetMuted(bool value)
        {
            if (AudioSettings != null)
            {
                AudioSettings.SetMuted(value);
                return;
            }

            PlayerPrefs.SetInt(MutedKey, value ? 1 : 0);
            PlayerPrefs.Save();
            ApplyPlaceholderAudio();
        }

        public void SetMasterVolume(float value)
        {
            value = Mathf.Clamp01(value);
            
            if (AudioSettings != null)
            {
                AudioSettings.SetMasterVolume(value);
                return;
            }

            PlayerPrefs.SetFloat(VolumeKey, value);
            PlayerPrefs.Save();
            ApplyPlaceholderAudio();
        }

        private void ApplyPlaceholderAudio()
        {
            AudioListener.volume = Muted ? 0f : MasterVolume;
        }
    }
}
