using System;
using UnityEngine;
using UnityEngine.Audio;

namespace HorrorCafe.Audio
{
    public sealed class AudioSettingsController : MonoBehaviour
    {
        private const string MuteKey = "HorrorCafe.Audio.Muted";
        private static AudioSettingsController instance;

        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private MixerParameter[] mixerParameters =
        {
            new MixerParameter(AudioMixerChannel.Master, "MasterVolume", 1f),
            new MixerParameter(AudioMixerChannel.Player, "PlayerVolume", 1f),
            new MixerParameter(AudioMixerChannel.Footsteps, "FootstepsVolume", 1f),
            new MixerParameter(AudioMixerChannel.Interactions, "InteractionsVolume", 1f),
            new MixerParameter(AudioMixerChannel.Environment, "EnvironmentVolume", 1f),
            new MixerParameter(AudioMixerChannel.Ambient, "AmbientVolume", 1f),
            new MixerParameter(AudioMixerChannel.Horror, "HorrorVolume", 1f),
            new MixerParameter(AudioMixerChannel.UI, "UIVolume", 1f)
        };

        private bool muted;
        public static AudioSettingsController Instance => instance;
        public bool Muted => muted;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
            ApplyAll();
        }

        private void OnDestroy()
        {
            if (instance == this) 
                instance = null;
        }

        public float GetVolume(AudioMixerChannel channel)
        {
            var parameter = FindParameter(channel);
            return parameter == null ? 1f : parameter.Volume;
        }

        public void SetVolume(AudioMixerChannel channel, float normalizedVolume)
        {
            var parameter = FindParameter(channel);
            
            if (parameter == null)
                return;
            
            parameter.Volume = Mathf.Clamp01(normalizedVolume);
            Apply(parameter);
            Save(parameter);
        }

        public void SetMasterVolume(float value)
        {
            SetVolume(AudioMixerChannel.Master, value);
        }

        public void SetPlayerVolume(float value)
        {
            SetVolume(AudioMixerChannel.Player, value);
        }

        public void SetFootstepsVolume(float value)
        {
            SetVolume(AudioMixerChannel.Footsteps, value);
        }

        public void SetInteractionsVolume(float value)
        {
            SetVolume(AudioMixerChannel.Interactions, value);
        }

        public void SetEnvironmentVolume(float value)
        {
            SetVolume(AudioMixerChannel.Environment, value);
        }

        public void SetAmbientVolume(float value)
        {
            SetVolume(AudioMixerChannel.Ambient, value);
        }

        public void SetHorrorVolume(float value)
        {
            SetVolume(AudioMixerChannel.Horror, value);
        }

        public void SetUIVolume(float value)
        {
            SetVolume(AudioMixerChannel.UI, value);
        }

        public void SetMuted(bool value)
        {
            muted = value;
            ApplyGlobalListenerVolume();
            PlayerPrefs.SetInt(MuteKey, muted ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void Load()
        {
            muted = PlayerPrefs.GetInt(MuteKey, 0) == 1;
            if (mixerParameters == null)
            {
                return;
            }
            for (var i = 0; i < mixerParameters.Length; i++)
            {
                var parameter = mixerParameters[i];
                if (parameter == null)
                {
                    continue;
                }
                parameter.Volume = PlayerPrefs.GetFloat(GetVolumeKey(parameter.Channel), parameter.DefaultVolume);
            }
        }

        private void ApplyAll()
        {
            if (mixerParameters == null)
            {
                ApplyGlobalListenerVolume();
                return;
            }
            for (var i = 0; i < mixerParameters.Length; i++)
            {
                Apply(mixerParameters[i]);
            }
        }

        private void Apply(MixerParameter parameter)
        {
            if (parameter != null && parameter.Channel == AudioMixerChannel.Master)
            {
                if (audioMixer != null && !string.IsNullOrWhiteSpace(parameter.ParameterName))
                    audioMixer.SetFloat(parameter.ParameterName, 0f);

                ApplyGlobalListenerVolume();
                return;
            }

            if (audioMixer == null || parameter == null || string.IsNullOrWhiteSpace(parameter.ParameterName))
            {
                return;
            }
            audioMixer.SetFloat(parameter.ParameterName, NormalizedToDecibels(parameter.Volume));
        }

        private void Save(MixerParameter parameter)
        {
            PlayerPrefs.SetFloat(GetVolumeKey(parameter.Channel), parameter.Volume);
            PlayerPrefs.Save();
        }

        private MixerParameter FindParameter(AudioMixerChannel channel)
        {
            if (mixerParameters == null)
            {
                return null;
            }
            for (var i = 0; i < mixerParameters.Length; i++)
            {
                if (mixerParameters[i] != null && mixerParameters[i].Channel == channel)
                {
                    return mixerParameters[i];
                }
            }
            return null;
        }

        private static float NormalizedToDecibels(float value)
        {
            if (value <= 0.0001f)
            {
                return -80f;
            }
            return Mathf.Log10(value) * 20f;
        }

        private static string GetVolumeKey(AudioMixerChannel channel)
        {
            return $"HorrorCafe.Audio.{channel}.Volume";
        }

        private void ApplyGlobalListenerVolume()
        {
            var master = FindParameter(AudioMixerChannel.Master);
            var volume = master == null ? 1f : master.Volume;
            AudioListener.volume = muted ? 0f : volume;
            AudioListener.pause = false;
        }

        [Serializable]
        private sealed class MixerParameter
        {
            [SerializeField] private AudioMixerChannel channel;
            [SerializeField] private string parameterName;
            [SerializeField, Range(0f, 1f)] private float defaultVolume = 1f;
            [SerializeField, Range(0f, 1f)] private float volume = 1f;
            public MixerParameter(AudioMixerChannel channel, string parameterName, float defaultVolume)
            {
                this.channel = channel;
                this.parameterName = parameterName;
                this.defaultVolume = defaultVolume;
                volume = defaultVolume;
            }
            public AudioMixerChannel Channel => channel;
            public string ParameterName => parameterName;
            public float DefaultVolume => defaultVolume;
            public float Volume
            {
                get => volume;
                set => volume = Mathf.Clamp01(value);
            }
        }
    }
}
