using UnityEngine;

namespace HorrorCafe.Interaction
{
    public readonly struct InteractionContext
    {
        public InteractionContext(GameObject actor, Camera camera, Transform holdPoint)
        {
            Actor = actor;
            Camera = camera;
            HoldPoint = holdPoint;
        }

        public GameObject Actor { get; }

        public Camera Camera { get; }

        public Transform HoldPoint { get; }
    }
}
