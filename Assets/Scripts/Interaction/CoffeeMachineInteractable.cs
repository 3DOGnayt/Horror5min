using System.Collections;
using System;
using DG.Tweening;
using UnityEngine;

namespace HorrorCafe.Interaction
{
    public sealed class CoffeeMachineInteractable : InteractableBase, IHeldFocusPreview
    {
        private enum CoffeeMachineState
        {
            Empty,
            CupPlaced,
            Brewing,
            NeedsLid,
            Ready
        }

        [Header("Slots")]
        [SerializeField] private Transform cupSlot;
        [SerializeField] private Vector3 cupLocalEulerOffset;
        [SerializeField] private Transform lidSlot;
        [SerializeField] private Vector3 lidLocalEulerOffset;
        [SerializeField] private Transform readyParent;

        [Header("Prompts")]
        [SerializeField] private string needCupPrompt = "[E] Need cup";
        [SerializeField] private string placeCupPrompt = "[E] Place cup";
        [SerializeField] private string brewPrompt = "[E] Brew coffee";
        [SerializeField] private string brewingPrompt = "Brewing...";
        [SerializeField] private string needLidPrompt = "[E] Need lid";
        [SerializeField] private string placeLidPrompt = "[E] Place lid";
        [SerializeField] private string takeCoffeePrompt = "[E] Take coffee";

        [Header("Timing")]
        [SerializeField, Min(0.1f)] private float brewDuration = 3f;

        [Header("Visuals")]
        [SerializeField] private Transform liquidTransform;
        [SerializeField] private Vector3 liquidEmptyLocalPosition;
        [SerializeField] private Vector3 liquidFullLocalPosition;
        [SerializeField] private Vector3 liquidEmptyLocalScale = Vector3.one;
        [SerializeField] private Vector3 liquidFullLocalScale = Vector3.one;
        [SerializeField] private ParticleSystem[] pourParticles;
        [SerializeField] private ParticleSystem[] steamParticles;

        private CoffeeMachineState state;
        private InspectablePickupObject placedCup;
        private InspectablePickupObject placedLid;
        private Coroutine brewRoutine;
        private InspectablePickupObject previewHeldPickup;
        private Tween liquidTween;

        public event Action<InspectablePickupObject> CupPlaced;
        public event Action BrewingStarted;
        public event Action BrewingFinishedNeedsLid;
        public event Action<InspectablePickupObject> LidPlaced;
        public event Action<InspectablePickupObject> CoffeeReady;
        public event Action<InspectablePickupObject> ReadyCoffeePickedUp;

        public override string Prompt => GetPrompt(previewHeldPickup);
        public override bool CanInteract => base.CanInteract && state != CoffeeMachineState.Ready;

        private void Awake()
        {
            ResetMachineVisuals();
        }

        private void OnDestroy()
        {
            if (brewRoutine != null)
                StopCoroutine(brewRoutine);

            liquidTween?.Kill();
        }

        public void SetHeldFocusPreview(IInteractionFocusLock heldFocus)
        {
            previewHeldPickup = heldFocus as InspectablePickupObject;
        }

        public override void Interact(InteractionContext context)
        {
            var heldPickup = context.HeldPickup;

            if (state == CoffeeMachineState.Empty)
            {
                if (TryPlaceHeldPickup(context, heldPickup, CoffeePickupKind.Cup))
                    state = CoffeeMachineState.CupPlaced;

                base.Interact(context);
                return;
            }

            if (state == CoffeeMachineState.CupPlaced)
            {
                StartBrewing();
                base.Interact(context);
                return;
            }

            if (state == CoffeeMachineState.NeedsLid)
            {
                if (TryPlaceHeldPickup(context, heldPickup, CoffeePickupKind.Lid))
                    MakeReady();

                base.Interact(context);
            }
        }

        public bool TryPlaceReleasedPickup(InspectablePickupObject pickup, CoffeePickupKind expectedKind)
        {
            if (pickup == null || !CanAcceptReleasedPickup(pickup, expectedKind))
                return false;

            if (expectedKind == CoffeePickupKind.Cup)
            {
                PlaceCup(pickup);
                state = CoffeeMachineState.CupPlaced;
                return true;
            }

            if (expectedKind == CoffeePickupKind.Lid)
            {
                PlaceLid(pickup);
                MakeReady();
                return true;
            }

            return false;
        }

        private string GetPrompt(InspectablePickupObject heldPickup)
        {
            if (state == CoffeeMachineState.Brewing)
                return brewingPrompt;

            if (state == CoffeeMachineState.Empty)
                return IsPickupKind(heldPickup, CoffeePickupKind.Cup) ? placeCupPrompt : needCupPrompt;

            if (state == CoffeeMachineState.CupPlaced)
                return brewPrompt;

            if (state == CoffeeMachineState.NeedsLid)
                return IsPickupKind(heldPickup, CoffeePickupKind.Lid) ? placeLidPrompt : needLidPrompt;

            return string.Empty;
        }

        private bool TryPlaceHeldPickup(InteractionContext context, InspectablePickupObject pickup, CoffeePickupKind expectedKind)
        {
            if (pickup == null || !IsPickupKind(pickup, expectedKind))
                return false;

            if (expectedKind == CoffeePickupKind.Cup)
                PlaceCup(pickup);
            else if (expectedKind == CoffeePickupKind.Lid)
                PlaceLid(pickup);
            else
                return false;

            context.SetHeldFocus(null);
            return true;
        }

        private bool CanAcceptReleasedPickup(InspectablePickupObject pickup, CoffeePickupKind expectedKind)
        {
            if (!IsPickupKind(pickup, expectedKind))
                return false;

            if (expectedKind == CoffeePickupKind.Cup)
                return state == CoffeeMachineState.Empty;

            if (expectedKind == CoffeePickupKind.Lid)
                return state == CoffeeMachineState.NeedsLid;

            return false;
        }

        private void PlaceCup(InspectablePickupObject cup)
        {
            placedCup = cup;
            placedCup.SnapTo(cupSlot, transform, false, cupLocalEulerOffset);
            placedCup.SetPrompt(takeCoffeePrompt);
            ResetMachineVisuals();
            CupPlaced?.Invoke(placedCup);
        }

        private void PlaceLid(InspectablePickupObject lid)
        {
            placedLid = lid;
            var parent = placedCup != null ? placedCup.transform : transform;
            placedLid.SnapTo(lidSlot, parent, false, lidLocalEulerOffset);
            placedLid.SetCollidersEnabled(false);
            LidPlaced?.Invoke(placedLid);
        }

        private void StartBrewing()
        {
            if (brewRoutine != null)
                return;

            state = CoffeeMachineState.Brewing;
            SetParticlesActive(pourParticles, true);
            SetParticlesActive(steamParticles, true);
            FillLiquid();
            brewRoutine = StartCoroutine(Brew());
            BrewingStarted?.Invoke();
        }

        private IEnumerator Brew()
        {
            yield return new WaitForSeconds(brewDuration);
            brewRoutine = null;
            state = CoffeeMachineState.NeedsLid;
            liquidTween?.Kill();
            SetLiquidFull();
            SetParticlesActive(pourParticles, false);
            SetParticlesActive(steamParticles, true);
            BrewingFinishedNeedsLid?.Invoke();
        }

        private void MakeReady()
        {
            state = CoffeeMachineState.Ready;
            ResetMachineVisuals();

            if (placedCup == null)
                return;

            placedCup.SnapTo(cupSlot, readyParent != null ? readyParent : transform.parent, true, cupLocalEulerOffset);
            placedCup.SetCollidersEnabled(true);
            placedCup.SetPrompt(takeCoffeePrompt);
            SetPickupKind(placedCup, CoffeePickupKind.ReadyCoffee);
            placedCup.PickedUp += OnReadyCoffeePickedUp;
            CoffeeReady?.Invoke(placedCup);
        }

        private static bool IsPickupKind(InspectablePickupObject pickup, CoffeePickupKind expectedKind)
        {
            if (pickup == null)
                return false;

            var tag = pickup.GetComponentInChildren<CoffeePickupTag>(true);
            return tag != null && tag.Kind == expectedKind;
        }

        private static void SetPickupKind(InspectablePickupObject pickup, CoffeePickupKind kind)
        {
            if (pickup == null)
                return;

            var tag = pickup.GetComponentInChildren<CoffeePickupTag>(true);
            if (tag != null)
                tag.SetKind(kind);
        }

        private void OnReadyCoffeePickedUp(InspectablePickupObject pickup)
        {
            if (pickup != null)
                pickup.PickedUp -= OnReadyCoffeePickedUp;

            placedCup = null;
            placedLid = null;
            state = CoffeeMachineState.Empty;
            ResetMachineVisuals();
            ReadyCoffeePickedUp?.Invoke(pickup);
        }

        private void ResetMachineVisuals()
        {
            liquidTween?.Kill();
            SetLiquidEmpty();
            SetParticlesActive(pourParticles, false);
            SetParticlesActive(steamParticles, false);
        }

        private void FillLiquid()
        {
            if (liquidTransform == null)
                return;

            liquidTransform.gameObject.SetActive(true);
            liquidTween?.Kill();
            liquidTween = DOTween.Sequence()
                .Join(liquidTransform.DOLocalMove(liquidFullLocalPosition, brewDuration))
                .Join(liquidTransform.DOScale(liquidFullLocalScale, brewDuration))
                .SetEase(Ease.Linear)
                .OnComplete(() => liquidTween = null);
        }

        private void SetLiquidEmpty()
        {
            if (liquidTransform == null)
                return;

            liquidTransform.localPosition = liquidEmptyLocalPosition;
            liquidTransform.localScale = liquidEmptyLocalScale;
            liquidTransform.gameObject.SetActive(false);
        }

        private void SetLiquidFull()
        {
            if (liquidTransform == null)
                return;

            liquidTransform.localPosition = liquidFullLocalPosition;
            liquidTransform.localScale = liquidFullLocalScale;
            liquidTransform.gameObject.SetActive(true);
        }

        private static void SetParticlesActive(ParticleSystem[] particles, bool active)
        {
            if (particles == null)
                return;

            foreach (var particle in particles)
            {
                if (particle == null)
                    continue;

                if (active)
                    particle.Play();
                else
                    particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }
}
