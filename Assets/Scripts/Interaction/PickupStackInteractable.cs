using System.Collections.Generic;
using UnityEngine;

namespace HorrorCafe.Interaction
{
    public sealed class PickupStackInteractable : InteractableBase, IRequiresEmptyHands
    {
        [SerializeField] private string takePrompt = "[E] Take cup";
        [SerializeField] private List<InspectablePickupObject> items = new List<InspectablePickupObject>();
        [SerializeField] private bool autoCollectFromChildren;
        [SerializeField] private BoxCollider interactionCollider;

        public override string Prompt => takePrompt;
        public override bool CanInteract => base.CanInteract && HasItems;

        private bool HasItems
        {
            get
            {
                RemoveMissingItems();
                return items.Count > 0;
            }
        }

        private void Awake()
        {
            if (interactionCollider == null)
                interactionCollider = GetComponent<BoxCollider>();

            if (autoCollectFromChildren && items.Count == 0)
                items.AddRange(GetComponentsInChildren<InspectablePickupObject>(true));

            foreach (var item in items)
                SetItemStacked(item);

            RefreshInteractionCollider();
        }

        public override void Interact(InteractionContext context)
        {
            var item = PopItem();
            if (item == null)
                return;

            PrepareItemForPickup(item);
            if (item.TryPickUp(context))
            {
                if (item is IInteractionFocusLock focusLock)
                    context.SetHeldFocus(focusLock);
            }
            else
            {
                SetItemStacked(item);
                items.Add(item);
            }

            RefreshInteractionCollider();
            base.Interact(context);
        }

        private InspectablePickupObject PopItem()
        {
            RemoveMissingItems();
            if (items.Count == 0)
                return null;

            var index = items.Count - 1;
            var item = items[index];
            items.RemoveAt(index);
            return item;
        }

        private void RemoveMissingItems()
        {
            for (var i = items.Count - 1; i >= 0; i--)
            {
                if (items[i] == null)
                    items.RemoveAt(i);
            }
        }

        private void RefreshInteractionCollider()
        {
            if (interactionCollider == null)
                return;

            RemoveMissingItems();
            if (items.Count == 0)
            {
                interactionCollider.enabled = false;
                return;
            }

            interactionCollider.enabled = true;
            if (!TryGetItemsRendererBounds(out var localBounds))
                return;

            interactionCollider.center = localBounds.center;
            interactionCollider.size = localBounds.size;
        }

        private bool TryGetItemsRendererBounds(out Bounds bounds)
        {
            bounds = default;
            var hasBounds = false;

            foreach (var item in items)
            {
                if (item == null)
                    continue;

                var renderers = item.GetComponentsInChildren<Renderer>(true);
                foreach (var itemRenderer in renderers)
                {
                    if (itemRenderer == null)
                        continue;

                    var rendererBounds = GetRendererBoundsInColliderSpace(itemRenderer);
                    if (hasBounds)
                        bounds.Encapsulate(rendererBounds);
                    else
                    {
                        bounds = rendererBounds;
                        hasBounds = true;
                    }
                }
            }

            return hasBounds;
        }

        private Bounds GetRendererBoundsInColliderSpace(Renderer itemRenderer)
        {
            var colliderTransform = interactionCollider.transform;
            var rendererTransform = itemRenderer.transform;
            var rendererBounds = itemRenderer.localBounds;
            var min = rendererBounds.min;
            var max = rendererBounds.max;
            var bounds = new Bounds(ToColliderLocalPoint(min.x, min.y, min.z), Vector3.zero);

            bounds.Encapsulate(ToColliderLocalPoint(min.x, min.y, max.z));
            bounds.Encapsulate(ToColliderLocalPoint(min.x, max.y, min.z));
            bounds.Encapsulate(ToColliderLocalPoint(min.x, max.y, max.z));
            bounds.Encapsulate(ToColliderLocalPoint(max.x, min.y, min.z));
            bounds.Encapsulate(ToColliderLocalPoint(max.x, min.y, max.z));
            bounds.Encapsulate(ToColliderLocalPoint(max.x, max.y, min.z));
            bounds.Encapsulate(ToColliderLocalPoint(max.x, max.y, max.z));

            return bounds;

            Vector3 ToColliderLocalPoint(float x, float y, float z)
            {
                var worldPoint = rendererTransform.TransformPoint(new Vector3(x, y, z));
                return colliderTransform.InverseTransformPoint(worldPoint);
            }
        }

        private static void SetItemStacked(InspectablePickupObject item)
        {
            if (item == null)
                return;

            item.enabled = false;

            var body = item.GetComponent<Rigidbody>();
            if (body != null)
                body.isKinematic = true;

            SetCollidersEnabled(item, false);
        }

        private static void PrepareItemForPickup(InspectablePickupObject item)
        {
            item.gameObject.SetActive(true);
            item.transform.SetParent(null, true);

            var body = item.GetComponent<Rigidbody>();
            if (body != null)
                body.isKinematic = true;

            SetCollidersEnabled(item, true);
            item.enabled = true;
        }

        private static void SetCollidersEnabled(InspectablePickupObject item, bool enabled)
        {
            var colliders = item.GetComponentsInChildren<Collider>(true);
            foreach (var itemCollider in colliders)
                itemCollider.enabled = enabled;
        }
    }
}
