using UnityEngine;

namespace HorrorCafe.Story
{
    [System.Serializable]
    public sealed class DialogueLine
    {
        [SerializeField] private string speaker;
        [SerializeField, TextArea(2, 4)] private string text;
        [SerializeField, Min(0f)] private float delayBefore;
        [SerializeField, Min(0.1f)] private float duration = 2f;
        [SerializeField, Min(0f)] private float delayAfter;
        [SerializeField] private AudioClip voiceClip;

        public string Speaker => speaker;
        public string Text => text;
        public float DelayBefore => delayBefore;
        public float Duration => duration;
        public float DelayAfter => delayAfter;
        public AudioClip VoiceClip => voiceClip;
    }
}
