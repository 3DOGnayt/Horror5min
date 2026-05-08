using UnityEngine;

namespace HorrorCafe.Interaction
{
    public sealed class CoffeePickupTag : MonoBehaviour
    {
        [SerializeField] private CoffeePickupKind kind;

        public CoffeePickupKind Kind => kind;

        public void SetKind(CoffeePickupKind value)
        {
            kind = value;
        }
    }
}
