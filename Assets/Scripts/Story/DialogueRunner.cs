using System.Collections;
using HorrorCafe.UI;
using UnityEngine;

namespace HorrorCafe.Story
{
    public sealed class DialogueRunner : MonoBehaviour
    {
        [SerializeField] private AudioSource voiceSource;

        private Coroutine dialogueRoutine;

        public bool IsPlaying { get; private set; }

        public Coroutine Play(MonoBehaviour owner, DialogueLine[] lines)
        {
            if (owner == null || !isActiveAndEnabled)
                return null;

            Stop();
            dialogueRoutine = StartCoroutine(PlayRoutine(lines));
            return dialogueRoutine;
        }

        public void Stop()
        {
            if (dialogueRoutine != null)
            {
                StopCoroutine(dialogueRoutine);
                dialogueRoutine = null;
            }

            IsPlaying = false;
            PlayerMessageService.Instance?.HideMessage();

            if (voiceSource != null)
            {
                voiceSource.Stop();
                voiceSource.loop = false;
            }
        }

        private IEnumerator PlayRoutine(DialogueLine[] lines)
        {
            IsPlaying = true;

            if (lines != null)
            {
                foreach (var line in lines)
                    yield return PlayLine(line);
            }

            PlayerMessageService.Instance?.HideMessage();
            IsPlaying = false;
            dialogueRoutine = null;
        }

        private IEnumerator PlayLine(DialogueLine line)
        {
            if (line == null)
                yield break;

            if (line.DelayBefore > 0f)
                yield return new WaitForSeconds(line.DelayBefore);

            if (voiceSource != null && line.VoiceClip != null)
            {
                voiceSource.Stop();
                voiceSource.loop = false;
                voiceSource.clip = line.VoiceClip;
                voiceSource.Play();
            }

            var message = FormatMessage(line);
            PlayerMessageService.Instance?.Show(message, line.Duration);

            yield return new WaitForSeconds(line.Duration);

            if (line.DelayAfter > 0f)
                yield return new WaitForSeconds(line.DelayAfter);
        }

        private static string FormatMessage(DialogueLine line)
        {
            if (line == null)
                return string.Empty;

            if (string.IsNullOrWhiteSpace(line.Speaker))
                return line.Text;

            return line.Speaker + ": " + line.Text;
        }
    }
}
