using System;
using UnityEngine;

namespace HorrorCafe.Interaction
{
    public readonly struct InteractionContext
    {
        private readonly Action<IInteractionFocusLock> focusLockHandler;

        public InteractionContext(GameObject actor, Camera camera, Transform holdPoint, Action<IInteractionFocusLock> focusLockHandler = null)
        {
            Actor = actor;
            Camera = camera;
            HoldPoint = holdPoint;
            this.focusLockHandler = focusLockHandler;
        }

        public GameObject Actor { get; }

        public Camera Camera { get; }

        public Transform HoldPoint { get; }

        public void SetHeldFocus(IInteractionFocusLock focusLock)
        {
            focusLockHandler?.Invoke(focusLock);
        }
    }
}
