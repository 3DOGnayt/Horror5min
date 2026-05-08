using System;
using UnityEngine;

namespace HorrorCafe.Interaction
{
    public readonly struct InteractionContext
    {
        private readonly Action<IInteractionFocusLock> focusLockHandler;

        public InteractionContext(GameObject actor, Camera camera, Transform holdPoint, IInteractionFocusLock heldFocus = null, Action<IInteractionFocusLock> focusLockHandler = null)
        {
            Actor = actor;
            Camera = camera;
            HoldPoint = holdPoint;
            HeldFocus = heldFocus;
            this.focusLockHandler = focusLockHandler;
        }

        public GameObject Actor { get; }

        public Camera Camera { get; }

        public Transform HoldPoint { get; }

        public IInteractionFocusLock HeldFocus { get; }

        public InspectablePickupObject HeldPickup => HeldFocus as InspectablePickupObject;

        public void SetHeldFocus(IInteractionFocusLock focusLock)
        {
            focusLockHandler?.Invoke(focusLock);
        }
    }
}
