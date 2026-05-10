using System.Collections;
using UnityEngine;

namespace HorrorCafe.Story
{
    public sealed class ObjectStateStoryStep : StoryStep
    {
        [SerializeField] private GameObject[] enableObjects;
        [SerializeField] private GameObject[] disableObjects;
        [SerializeField, Min(0f)] private float delayAfter;
        [SerializeField] private DialogueLine[] linesAfter;

        protected override IEnumerator Execute(StoryScenarioController controller)
        {
            SetObjects(enableObjects, true);
            SetObjects(disableObjects, false);

            if (delayAfter > 0f)
                yield return new WaitForSeconds(delayAfter);

            if (controller.DialogueRunner != null && linesAfter != null && linesAfter.Length > 0)
            {
                controller.DialogueRunner.Play(this, linesAfter);
                while (controller.DialogueRunner.IsPlaying)
                    yield return null;
            }
        }

        private static void SetObjects(GameObject[] objects, bool active)
        {
            if (objects == null)
                return;

            foreach (var item in objects)
            {
                if (item != null)
                    item.SetActive(active);
            }
        }
    }
}
