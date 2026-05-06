using UnityEngine;
using UnityEngine.Rendering;

namespace HorrorCafe.Effects
{
    public sealed class VhsPostProcessController : MonoBehaviour
    {
        [SerializeField] private Volume volume;
        [SerializeField, Range(0f, 1f)] private float normalWeight = 1f;
        [SerializeField, Range(0f, 1f)] private float horrorWeight = 1f;

        private void Reset()
        {
            volume = GetComponent<Volume>();
        }

        private void Awake()
        {
            SetNormal();
        }

        public void SetNormal()
        {
            SetWeight(normalWeight);
        }

        public void SetHorror()
        {
            SetWeight(horrorWeight);
        }

        public void SetWeight(float weight)
        {
            if (volume == null)
                return;

            volume.weight = Mathf.Clamp01(weight);
        }
    }
}
