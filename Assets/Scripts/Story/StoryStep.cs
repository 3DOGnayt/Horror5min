using System;
using System.Collections;
using UnityEngine;

namespace HorrorCafe.Story
{
    public abstract class StoryStep : MonoBehaviour
    {
        [SerializeField] private string stepId;
        [SerializeField] private bool skip;

        private Coroutine activeRoutine;

        public string StepId => string.IsNullOrWhiteSpace(stepId) ? name : stepId;
        public bool IsRunning { get; private set; }
        public bool IsCompleted { get; private set; }

        public event Action<StoryStep> Completed;

        public void Run(StoryScenarioController controller)
        {
            if (skip)
            {
                Complete();
                return;
            }

            if (IsRunning || IsCompleted)
                return;

            IsRunning = true;
            activeRoutine = StartCoroutine(RunRoutine(controller));
        }

        public void ResetStep()
        {
            if (activeRoutine != null)
            {
                StopCoroutine(activeRoutine);
                activeRoutine = null;
            }

            IsRunning = false;
            IsCompleted = false;
        }

        protected void Complete()
        {
            if (IsCompleted)
                return;

            if (activeRoutine != null)
            {
                StopCoroutine(activeRoutine);
                activeRoutine = null;
            }

            IsRunning = false;
            IsCompleted = true;
            Completed?.Invoke(this);
        }

        protected abstract IEnumerator Execute(StoryScenarioController controller);

        private IEnumerator RunRoutine(StoryScenarioController controller)
        {
            yield return Execute(controller);
            activeRoutine = null;
            Complete();
        }
    }
}
