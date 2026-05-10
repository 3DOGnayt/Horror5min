using System.Collections;
using UnityEngine;

namespace HorrorCafe.Story
{
    public sealed class AudioStoryStep : StoryStep
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip clip;
        [SerializeField] private bool loop;
        [SerializeField] private bool stopInstead;
        [SerializeField, Min(0f)] private float delayAfter;

        protected override IEnumerator Execute(StoryScenarioController controller)
        {
            if (audioSource != null)
            {
                if (stopInstead)
                {
                    audioSource.Stop();
                }
                else if (clip != null)
                {
                    audioSource.clip = clip;
                    audioSource.loop = loop;
                    audioSource.Play();
                }
            }

            if (delayAfter > 0f)
                yield return new WaitForSeconds(delayAfter);
        }
    }
}
