using System.Collections.Generic;
using UnityEngine;

namespace HorrorCafe.Story
{
    public sealed class StoryScenarioController : MonoBehaviour
    {
        [SerializeField] private bool playOnStart = true;
        [SerializeField] private DialogueRunner dialogueRunner;
        [SerializeField] private StoryStep[] steps;

        private int currentIndex = -1;

        public DialogueRunner DialogueRunner => dialogueRunner;
        public StoryStep CurrentStep => steps != null && currentIndex >= 0 && currentIndex < steps.Length ? steps[currentIndex] : null;

        private void Reset()
        {
            dialogueRunner = FindObjectOfType<DialogueRunner>();
            CollectStepsFromChildren();
        }

        private void Awake()
        {
            if (dialogueRunner == null)
                dialogueRunner = FindObjectOfType<DialogueRunner>();
        }

        private void Start()
        {
            if (playOnStart)
                Play();
        }

        public void Play()
        {
            if (steps == null || steps.Length == 0)
                CollectStepsFromChildren();

            foreach (var step in EnumerateSteps())
            {
                step.ResetStep();
                step.Completed -= OnStepCompleted;
                step.Completed += OnStepCompleted;
            }

            currentIndex = -1;
            PlayNext();
        }

        public void PlayNext()
        {
            currentIndex++;

            if (currentIndex >= steps.Length)
                return;

            var step = steps[currentIndex];
            if (step == null)
            {
                PlayNext();
                return;
            }

            Debug.Log("Story step started: " + step.StepId, step);
            step.Run(this);
        }

        private void OnStepCompleted(StoryStep step)
        {
            Debug.Log("Story step completed: " + step.StepId, step);
            PlayNext();
        }

        private IEnumerable<StoryStep> EnumerateSteps()
        {
            if (steps == null)
                yield break;

            foreach (var step in steps)
            {
                if (step != null)
                    yield return step;
            }
        }

        [ContextMenu("Collect Steps From Children")]
        public void CollectStepsFromChildren()
        {
            steps = GetComponentsInChildren<StoryStep>(true);
        }
    }
}
