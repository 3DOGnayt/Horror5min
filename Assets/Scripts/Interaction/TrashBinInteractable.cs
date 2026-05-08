using DG.Tweening;
using UnityEngine;

namespace HorrorCafe.Interaction
{
    public sealed class TrashBinInteractable : InteractableBase
    {
        [SerializeField] private Transform lid;
        [SerializeField] private string openPrompt;
        [SerializeField] private string closePrompt;
        [SerializeField] private Vector3 closedLocalEuler = Vector3.zero;
        [SerializeField] private Vector3 openLocalEuler = new Vector3(-75f, 0f, 0f);
        [SerializeField, Min(0.01f)] private float animationDuration = 0.35f;
        [SerializeField] private Ease animationEase = Ease.OutSine;
        [SerializeField] private ParticleSystem[] particles;
        [SerializeField, Min(0f)] private float particlesStartDelay;
        [SerializeField] private bool startOpen;
        [SerializeField] private bool useCurrentRotationAsClosed = true;

        private Tween lidTween;
        private Tween particlesTween;
        private bool isOpen;
        private bool isAnimating;

        public bool IsOpen => isOpen;
        public override string Prompt => isOpen ? closePrompt : openPrompt;
        public override bool CanInteract => base.CanInteract && lid != null;

        private void Awake()
        {
            if (lid == null)
                lid = transform;

            if (useCurrentRotationAsClosed)
                closedLocalEuler = lid.localEulerAngles;

            isOpen = startOpen;
            lid.localRotation = Quaternion.Euler(isOpen ? openLocalEuler : closedLocalEuler);
            SetParticlesActive(isOpen);
        }

        private void OnDestroy()
        {
            lidTween?.Kill();
            particlesTween?.Kill();
        }

        public override void Interact(InteractionContext context)
        {
            SetOpen(!isOpen);
            base.Interact(context);
        }

        public void SetOpen(bool value)
        {
            if (lid == null || isAnimating || isOpen == value)
                return;

            isOpen = value;
            isAnimating = true;
            SetParticlesActive(isOpen);

            lidTween?.Kill();
            lidTween = lid.DOLocalRotate(isOpen ? openLocalEuler : closedLocalEuler, animationDuration)
                .SetEase(animationEase)
                .OnComplete(() =>
                {
                    isAnimating = false;
                    lidTween = null;
                });
        }

        private void SetParticlesActive(bool value)
        {
            particlesTween?.Kill();
            particlesTween = null;

            if (particles == null)
                return;

            if (value && particlesStartDelay > 0f)
            {
                particlesTween = DOVirtual.DelayedCall(particlesStartDelay, PlayParticles);
                return;
            }

            if (value)
            {
                PlayParticles();
                return;
            }

            StopParticles();
        }

        private void PlayParticles()
        {
            foreach (var particle in particles)
            {
                if (particle == null)
                    continue;

                particle.Play();
            }
        }

        private void StopParticles()
        {
            foreach (var particle in particles)
            {
                if (particle == null)
                    continue;

                particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }
}
