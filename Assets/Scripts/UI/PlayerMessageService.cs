using System.Collections;
using TMPro;
using UnityEngine;

namespace HorrorCafe.UI
{
    public sealed class PlayerMessageService : MonoBehaviour
    {
        [SerializeField] private TMP_Text messageText;
        [SerializeField, Min(0.1f)] private float defaultDuration = 2f;

        private Coroutine hideRoutine;

        public static PlayerMessageService Instance { get; private set; }
        public bool IsShowing { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple PlayerMessageService instances found. Using the latest one.", this);
            }

            Instance = this;
            HideMessage();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void Show(string message)
        {
            Show(message, defaultDuration);
        }

        public void Show(string message, float duration)
        {
            if (messageText == null)
            {
                Debug.Log(message);
                IsShowing = false;
                return;
            }

            IsShowing = !string.IsNullOrWhiteSpace(message);
            messageText.text = message;
            messageText.gameObject.SetActive(IsShowing);

            if (hideRoutine != null)
                StopCoroutine(hideRoutine);

            hideRoutine = StartCoroutine(HideAfter(duration));
        }

        public void HideMessage()
        {
            if (hideRoutine != null)
            {
                StopCoroutine(hideRoutine);
                hideRoutine = null;
            }

            if (messageText == null)
                return;

            IsShowing = false;
            messageText.text = string.Empty;
            messageText.gameObject.SetActive(false);
        }

        private IEnumerator HideAfter(float duration)
        {
            yield return new WaitForSeconds(duration);
            hideRoutine = null;
            HideMessage();
        }
    }
}
