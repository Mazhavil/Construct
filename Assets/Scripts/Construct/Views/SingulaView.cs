using Attributes;
using Construct.Components;
using Construct.Model;
using Leopotam.EcsLite;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Construct.Views
{
    public sealed class SingulaView : MonoBehaviour
    {
        [ReadOnly] public int Id;
        [ReadOnly] public string Name;
        [ReadOnly] public int EcsEntity;
        [ReadOnly] public Pimple[] Pimples;

        private XRGrabInteractable _xrGrabInteractable;

        public XRGrabInteractable SetXrGrabActions(EcsWorld world, InteractionLayerMask interactionLayerMask)
        {
            _xrGrabInteractable = GetComponent<XRGrabInteractable>();
            _xrGrabInteractable.movementType = XRBaseInteractable.MovementType.VelocityTracking;
            _xrGrabInteractable.interactionLayers = interactionLayerMask;
            _xrGrabInteractable.hoverEntered.AddListener(_ => {
                if (!world.GetPool<StartFocus>().Has(EcsEntity)) world.GetPool<StartFocus>().Add(EcsEntity);
            });

            _xrGrabInteractable.hoverExited.AddListener(_ => {
                if (!world.GetPool<EndFocus>().Has(EcsEntity)) world.GetPool<EndFocus>().Add(EcsEntity);
            });

            _xrGrabInteractable.selectEntered.AddListener(args => {
                var colliders = args.interactableObject.colliders;
                var handPosition = args.interactorObject.transform.position;
                var shortestDistance = float.MaxValue;
                var closestCollider = colliders[0];

                foreach (var collider in colliders) {
                    var distance = Vector3.Distance(handPosition, collider.transform.position);
                    if (distance <= shortestDistance) {
                        shortestDistance = distance;
                        closestCollider = collider;
                    }
                }

                _xrGrabInteractable.attachTransform = closestCollider.transform;

                if (!world.GetPool<TakeToHand>().Has(EcsEntity)) world.GetPool<TakeToHand>().Add(EcsEntity);
            });

            _xrGrabInteractable.selectExited.AddListener(_ => {
                if (!world.GetPool<ReleaseFromHand>().Has(EcsEntity)) world.GetPool<ReleaseFromHand>().Add(EcsEntity);
            });

            _xrGrabInteractable.activated.AddListener(_ => {
                if (!world.GetPool<JoinSingula>().Has(EcsEntity)) world.GetPool<JoinSingula>().Add(EcsEntity);
            });

            return _xrGrabInteractable;
        }
    }
}
