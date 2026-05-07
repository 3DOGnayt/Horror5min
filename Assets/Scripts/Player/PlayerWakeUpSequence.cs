using DG.Tweening;
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

        private Sequence wakeSequence;

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
            wakeSequence?.Kill();
            StartWakeTween();
        }

        private void OnDestroy()
        {
            wakeSequence?.Kill();
        }

        private void StartWakeTween()
        {
            SetControls(false);
            SetStandUpAnimatorEnabled(false);
            wakeStarted?.Invoke();

            ApplyView(seatedLocalPosition, seatedLocalEuler);

            wakeSequence = DOTween.Sequence(this);
            wakeSequence.AppendInterval(seatedDelay);
            wakeSequence.AppendCallback(() =>
            {
                PlayStandUpAnimation();
                standUpStarted?.Invoke();
            });

            if (viewPivot != null)
            {
                wakeSequence.Append(viewPivot.DOLocalMove(standingLocalPosition, standUpDuration)
                    .SetEase(Ease.InOutSine));
                wakeSequence.Join(viewPivot.DOLocalRotate(standingLocalEuler, standUpDuration)
                    .SetEase(Ease.InOutSine));
            }
            else
            {
                wakeSequence.AppendInterval(standUpDuration);
            }

            wakeSequence.OnComplete(FinishWakeUp);
        }

        private void FinishWakeUp()
        {
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
