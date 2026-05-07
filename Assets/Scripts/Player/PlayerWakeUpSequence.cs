using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace HorrorCafe.Player
{
    public sealed class PlayerWakeUpSequence : MonoBehaviour
    {
        [SerializeField] private HorrorPlayerController playerController;
        [SerializeField] private CameraLook cameraLook;
        [SerializeField] private Transform viewPivot;
        [SerializeField] private Animator characterAnimator;
        [SerializeField] private string standUpTrigger = "StandUp";
        [SerializeField] private Vector3 seatedLocalPosition = new Vector3(0f, 1.05f, 0f);
        [SerializeField] private Vector3 standingLocalPosition = new Vector3(0f, 1.7f, 0f);
        [SerializeField] private Vector3 seatedLocalEuler = new Vector3(8f, 0f, 0f);
        [SerializeField] private Vector3 standingLocalEuler = Vector3.zero;
        [SerializeField, Min(0f)] private float seatedDelay = 2f;
        [SerializeField, Min(0.01f)] private float standUpDuration = 2.2f;
        [SerializeField] private UnityEvent wakeStarted;
        [SerializeField] private UnityEvent standUpStarted;
        [SerializeField] private UnityEvent wakeFinished;

        private void Reset()
        {
            playerController = GetComponent<HorrorPlayerController>();
            cameraLook = GetComponentInChildren<CameraLook>();
            if (cameraLook != null)
            {
                viewPivot = cameraLook.transform;
            }
        }

        private void Start()
        {
            StartWakeUp();
        }

        public void StartWakeUp()
        {
            StopAllCoroutines();
            StartCoroutine(WakeRoutine());
        }

        private IEnumerator WakeRoutine()
        {
            SetControls(false);
            SetStandUpAnimatorEnabled(false);
            wakeStarted?.Invoke();

            ApplyView(seatedLocalPosition, seatedLocalEuler);

            if (seatedDelay > 0f)
                yield return new WaitForSeconds(seatedDelay);

            PlayStandUpAnimation();
            standUpStarted?.Invoke();

            var elapsed = 0f;
            
            while (elapsed < standUpDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.SmoothStep(0f, 1f, elapsed / standUpDuration);
                var position = Vector3.Lerp(seatedLocalPosition, standingLocalPosition, t);
                var rotation = Vector3.Lerp(seatedLocalEuler, standingLocalEuler, t);
                ApplyView(position, rotation);
                yield return null;
            }

            ApplyView(standingLocalPosition, standingLocalEuler);
            
            if (cameraLook != null) 
                cameraLook.SetPitch(standingLocalEuler.x);

            SetControls(true);
            wakeFinished?.Invoke();
        }

        private void ApplyView(Vector3 localPosition, Vector3 localEuler)
        {
            if (viewPivot == null)
                return;

            viewPivot.localPosition = localPosition;
            viewPivot.localRotation = Quaternion.Euler(localEuler);
        }

        private void PlayStandUpAnimation()
        {
            if (characterAnimator == null)
                return;

            SetStandUpAnimatorEnabled(true);

            if (string.IsNullOrWhiteSpace(standUpTrigger))
                return;

            if (HasTriggerParameter(standUpTrigger))
            {
                characterAnimator.ResetTrigger(standUpTrigger);
                characterAnimator.SetTrigger(standUpTrigger);
                return;
            }

            characterAnimator.Play(standUpTrigger, 0, 0f);
            characterAnimator.Update(0f);
        }

        private void SetStandUpAnimatorEnabled(bool enabled)
        {
            if (characterAnimator != null)
                characterAnimator.enabled = enabled;
        }

        private bool HasTriggerParameter(string parameterName)
        {
            foreach (var parameter in characterAnimator.parameters)
            {
                if (parameter.type == AnimatorControllerParameterType.Trigger &&
                    parameter.name == parameterName)
                    return true;
            }

            return false;
        }

        private void SetControls(bool enabled)
        {
            if (playerController != null) 
                playerController.ControlsEnabled = enabled;
            
            if (cameraLook != null) 
                cameraLook.LookEnabled = enabled;
        }
    }
}
