using System;
using UnityEngine;

namespace HorrorCafe.Story
{
    [RequireComponent(typeof(Collider))]
    public sealed class StoryTriggerZone : MonoBehaviour
    {
        [SerializeField] private bool requirePlayerTag;
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private bool triggerOnce = true;

        private bool triggered;

        public event Action Entered;

        private void Reset()
        {
            SetColliderTrigger();
        }

        private void OnValidate()
        {
            SetColliderTrigger();
        }

        public void ResetTrigger()
        {
            triggered = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (triggerOnce && triggered)
                return;

            if (requirePlayerTag && (other == null || !other.CompareTag(playerTag)))
                return;

            triggered = true;
            Entered?.Invoke();
        }

        private void SetColliderTrigger()
        {
            var triggerCollider = GetComponent<Collider>();
            if (triggerCollider != null)
                triggerCollider.isTrigger = true;
        }
    }
}
