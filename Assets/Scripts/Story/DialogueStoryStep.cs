using System.Collections;
using UnityEngine;

namespace HorrorCafe.Story
{
    public sealed class DialogueStoryStep : StoryStep
    {
        [SerializeField, Min(0f)] private float delayBefore;
        [SerializeField] private DialogueLine[] lines;
        [SerializeField, Min(0f)] private float delayAfter;

        protected override IEnumerator Execute(StoryScenarioController controller)
        {
            if (delayBefore > 0f)
                yield return new WaitForSeconds(delayBefore);

            if (controller.DialogueRunner != null && lines != null && lines.Length > 0)
            {
                controller.DialogueRunner.Play(this, lines);
                while (controller.DialogueRunner.IsPlaying)
                    yield return null;
            }

            if (delayAfter > 0f)
                yield return new WaitForSeconds(delayAfter);
        }
    }
}
