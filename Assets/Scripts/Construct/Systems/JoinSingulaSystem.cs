using System.Collections.Generic;
using System.Linq;
using Construct.Components;
using Construct.Model;
using Construct.Services;
using Construct.Views;
using Leopotam.EcsLite;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

namespace Construct.Systems
{
    public sealed class JoinSingulaSystem : IEcsRunSystem
    {
        private readonly EcsWorld _world;
        private readonly EcsFilter _joinSingulaFilter;
        private readonly EcsPool<JoinSingula> _joinSingulaPool;
        private readonly EcsPool<Singula> _singulaPool;
        private readonly EcsPool<InHand> _inHandPool;
        private readonly EcsPool<PossibleJoin> _possibleJoinPool;
        private readonly EcsPool<Conventus> _conventusPool;
        private readonly EcsPool<ReleaseFromHand> _releaseFromHandPool;
        private readonly EcsPool<InJoin> _inJoinPool;
        private readonly EcsPool<MetaSingula> _metaSingulaPool;
        private readonly EcsPool<DeleteSingula> _deleteSingulaPool;

        public JoinSingulaSystem(EcsWorld world)
        {
            _world = world;
            _joinSingulaFilter = _world.Filter<JoinSingula>().Inc<Singula>().Inc<InHand>().End();
            _joinSingulaPool = _world.GetPool<JoinSingula>();
            _singulaPool = _world.GetPool<Singula>();
            _inHandPool = _world.GetPool<InHand>();
            _possibleJoinPool = _world.GetPool<PossibleJoin>();
            _conventusPool = _world.GetPool<Conventus>();
            _releaseFromHandPool = _world.GetPool<ReleaseFromHand>();
            _inJoinPool = _world.GetPool<InJoin>();
            _metaSingulaPool = _world.GetPool<MetaSingula>();
            _deleteSingulaPool = _world.GetPool<DeleteSingula>();
        }

        public void Run (IEcsSystems systems)
        {
            foreach (var entity in _joinSingulaFilter) {
                ref var inHand = ref _inHandPool.Get(entity);

                if (inHand.PossibleJoinEcsEntity == -1) {
                    _joinSingulaPool.Del(entity);
                    _releaseFromHandPool.Add(entity);
                    continue;
                }

                ref var rightSingula = ref _singulaPool.Get(entity);
                ref var possibleJoin = ref _possibleJoinPool.Get(inHand.PossibleJoinEcsEntity);
                ref var leftSingula = ref _singulaPool.Get(inHand.PossibleJoinEcsEntity);
                ref var conventus = ref _conventusPool.Get(rightSingula.ConventusEcsEntity);

                rightSingula.Transform.position = possibleJoin.SingulaFrame.transform.position;
                rightSingula.Transform.rotation = possibleJoin.SingulaFrame.transform.rotation;

                var leftPimpleId = possibleJoin.PimpleIdSingulaFrame;
                var rightPimpleId = possibleJoin.PimplePairs[leftPimpleId];

                var leftJoin = conventus.Joins[leftSingula.Pimples[leftPimpleId].JoinId];
                var rightJoin = conventus.Joins[rightSingula.Pimples[rightPimpleId].JoinId];

                var nextJoinId = leftJoin.NextJoinIds.Intersect(rightJoin.NextJoinIds).First();
                var nextJoin = conventus.Joins[nextJoinId];

                // Собираем все точки соединений из левого соединения (не то что в руке)
                if (nextJoin.LeftPimples.Count == 0) {
                    if (leftJoin.LeftJoinId == 0 && leftJoin.RightJoinId == 0) {
                        nextJoin.LeftPimples.Add(new SingulaJoin() {
                            SingulaId = leftSingula.Id,
                            PimpleId = leftPimpleId
                        });
                    } else {
                        nextJoin.LeftPimples.AddRange(leftJoin.LeftPimples);
                        nextJoin.LeftPimples.AddRange(leftJoin.RightPimples);
                    }
                }

                // Собираем все точки соединений из правого соединения (то что в руке)
                if (nextJoin.RightPimples.Count == 0) {
                    if (rightJoin.LeftJoinId == 0 && rightJoin.RightJoinId == 0) {
                        nextJoin.RightPimples.Add(new SingulaJoin() {
                            SingulaId = rightSingula.Id,
                            PimpleId = rightPimpleId
                        });
                    } else {
                        nextJoin.RightPimples.AddRange(rightJoin.LeftPimples);
                        nextJoin.RightPimples.AddRange(rightJoin.RightPimples);
                    }
                }

                var isRightSingulaMeta = _metaSingulaPool.Has(entity);
                var isLeftSingulaMeta = _metaSingulaPool.Has(inHand.PossibleJoinEcsEntity);

                if (isRightSingulaMeta && isLeftSingulaMeta) {
                    ref var rightMeta = ref _metaSingulaPool.Get(entity);
                    ref var leftMeta = ref _metaSingulaPool.Get(inHand.PossibleJoinEcsEntity);

                    foreach (var inMetaSingulaEcsEntity in rightMeta.SingulaEcsEntities) {
                        ref var inMetaSingula = ref _singulaPool.Get(inMetaSingulaEcsEntity);
                        leftMeta.SingulaEcsEntities.Add(inMetaSingulaEcsEntity);
                        inMetaSingula.Transform.SetParent(leftSingula.Transform);
                        leftSingula.XRGrabInteractable.colliders.Add(inMetaSingula.Collider);

                        ref var inJoin = ref _inJoinPool.Get(inMetaSingulaEcsEntity);
                        inJoin.MetaSingulaEcsEntity = inHand.PossibleJoinEcsEntity;
                    }

                    leftSingula.Pimples = GetNewPimples(
                        rightSingula,
                        possibleJoin.PimplePairs[possibleJoin.PimpleIdSingulaFrame],
                        leftSingula,
                        possibleJoin.PimpleIdSingulaFrame,
                        nextJoinId,
                        leftSingula);

                    GameObject.Destroy(rightSingula.Transform.gameObject);
                    leftSingula.XRGrabInteractable.enabled = false;
                    leftSingula.XRGrabInteractable.enabled = true;
                    _deleteSingulaPool.Add(entity);
                } else if (isRightSingulaMeta) {
                    ref var rightMeta = ref _metaSingulaPool.Get(entity);
                    rightMeta.SingulaEcsEntities.Add(inHand.PossibleJoinEcsEntity);

                    GameObject.Destroy(leftSingula.SingulaView.GetComponent<XRSingleGrabFreeTransformer>());
                    GameObject.Destroy(leftSingula.XRGrabInteractable);
                    GameObject.Destroy(leftSingula.Transform.GetComponent<Rigidbody>());

                    rightSingula.XRGrabInteractable.enabled = false;
                    rightSingula.XRGrabInteractable.colliders.Add(leftSingula.Collider);
                    rightSingula.XRGrabInteractable.enabled = true;

                    leftSingula.Transform.SetParent(rightSingula.Transform);
                    leftSingula.XRGrabInteractable = null;

                    rightSingula.Pimples = GetNewPimples(
                        rightSingula,
                        possibleJoin.PimplePairs[possibleJoin.PimpleIdSingulaFrame],
                        leftSingula,
                        possibleJoin.PimpleIdSingulaFrame,
                        nextJoinId,
                        rightSingula);

                    _inJoinPool.Add(inHand.PossibleJoinEcsEntity);
                } else if (isLeftSingulaMeta) {
                    ref var leftMeta = ref _metaSingulaPool.Get(inHand.PossibleJoinEcsEntity);
                    leftMeta.SingulaEcsEntities.Add(entity);

                    GameObject.Destroy(rightSingula.SingulaView.GetComponent<XRSingleGrabFreeTransformer>());
                    GameObject.Destroy(rightSingula.XRGrabInteractable);
                    GameObject.Destroy(rightSingula.Transform.GetComponent<Rigidbody>());

                    leftSingula.XRGrabInteractable.enabled = false;
                    leftSingula.XRGrabInteractable.colliders.Add(rightSingula.Collider);
                    leftSingula.XRGrabInteractable.enabled = true;

                    rightSingula.Transform.SetParent(leftSingula.Transform);
                    rightSingula.XRGrabInteractable = null;

                    leftSingula.Pimples = GetNewPimples(
                        rightSingula,
                        possibleJoin.PimplePairs[possibleJoin.PimpleIdSingulaFrame],
                        leftSingula,
                        possibleJoin.PimpleIdSingulaFrame,
                        nextJoinId,
                        leftSingula);

                    _inJoinPool.Add(entity);
                } else {
                    var gameObject = new GameObject("MetaSingula");
                    gameObject.transform.position = leftSingula.Transform.position;
                    gameObject.transform.rotation = leftSingula.Transform.rotation;

                    var interactionLayerMask = leftSingula.XRGrabInteractable.interactionLayers;

                    GameObject.Destroy(leftSingula.SingulaView.GetComponent<XRSingleGrabFreeTransformer>());
                    GameObject.Destroy(rightSingula.SingulaView.GetComponent<XRSingleGrabFreeTransformer>());
                    GameObject.Destroy(leftSingula.XRGrabInteractable);
                    GameObject.Destroy(rightSingula.XRGrabInteractable);
                    GameObject.Destroy(leftSingula.SingulaView.GetComponent<Rigidbody>());
                    GameObject.Destroy(rightSingula.SingulaView.GetComponent<Rigidbody>());

                    leftSingula.XRGrabInteractable = null;
                    rightSingula.XRGrabInteractable = null;

                    gameObject.AddComponent<Rigidbody>();
                    var interactable = gameObject.AddComponent<XRGrabInteractable>();

                    // Нужно выключить элемент и потом включить, чтобы произошла регистрация колайдеров.
                    interactable.enabled = false;
                    interactable.colliders.Add(leftSingula.Collider);
                    interactable.colliders.Add(rightSingula.Collider);
                    interactable.enabled = true;

                    leftSingula.Transform.SetParent(gameObject.transform);
                    rightSingula.Transform.SetParent(gameObject.transform);

                    var metaSingulaView = gameObject.AddComponent<MetaSingulaView>();

                    var metaSingulaEntity = _world.NewEntity();
                    ref var meta = ref _metaSingulaPool.Add(metaSingulaEntity);
                    ref var metaSingula = ref _singulaPool.Add(metaSingulaEntity);

                    meta.MetaSingulaView = metaSingulaView;
                    meta.SingulaEcsEntities = new List<int>() {
                        entity,
                        inHand.PossibleJoinEcsEntity
                    };

                    metaSingula.Outline = null;
                    metaSingula.Id = MetaSingulaIdService.GetId();
                    metaSingula.Name = "meta_singula";
                    metaSingula.ConventusEcsEntity = rightSingula.ConventusEcsEntity;
                    metaSingula.Transform = gameObject.GetComponent<Transform>();
                    metaSingula.SingulaView = gameObject.AddComponent<SingulaView>();
                    metaSingula.XRGrabInteractable = metaSingula.SingulaView.SetXrGrabActions(_world, interactionLayerMask);
                    metaSingula.SingulaView.Id = metaSingula.Id;
                    metaSingula.SingulaView.Name = metaSingula.Name;
                    metaSingula.SingulaView.EcsEntity = metaSingulaEntity;

                    metaSingula.Pimples = GetNewPimples(
                        rightSingula,
                        possibleJoin.PimplePairs[possibleJoin.PimpleIdSingulaFrame],
                        leftSingula,
                        possibleJoin.PimpleIdSingulaFrame,
                        nextJoinId,
                        metaSingula);

                    metaSingula.SingulaView.Pimples = metaSingula.Pimples.Select(x => x.Value).ToArray();

                    ref var rightInJoin = ref _inJoinPool.Add(entity);
                    rightInJoin.MetaSingulaEcsEntity = metaSingulaEntity;

                    ref var leftInJoin = ref _inJoinPool.Add(inHand.PossibleJoinEcsEntity);
                    leftInJoin.MetaSingulaEcsEntity = metaSingulaEntity;
                }

                _joinSingulaPool.Del(entity);
                if (!_releaseFromHandPool.Has(entity)) _releaseFromHandPool.Add(entity);
            }
        }

        private Dictionary<int, Pimple> GetNewPimples(
            in Singula rightSingula,
            in int rightCommonPimpleId,
            in Singula leftSingula,
            in int leftCommonPimpleId,
            in int nextJoinId,
            in Singula metaSingula)
        {
            var pimples = new Dictionary<int, Pimple>(rightSingula.Pimples.Count + leftSingula.Pimples.Count - 1);
            var pimpleNewId = 1;

            foreach (var pimple in leftSingula.Pimples) {
                if (pimple.Key == leftCommonPimpleId) continue;

                var oldPosition = leftSingula.Transform.TransformPoint(pimple.Value.Position);
                pimples[pimpleNewId] = new Pimple() {
                    Id = pimpleNewId,
                    Position = metaSingula.Transform.InverseTransformPoint(oldPosition),
                    JoinId = pimple.Value.JoinId
                };

                pimpleNewId++;
            }

            foreach (var pimple in rightSingula.Pimples) {
                if (pimple.Key == rightCommonPimpleId) continue;

                var oldPosition = rightSingula.Transform.TransformPoint(pimple.Value.Position);
                pimples[pimpleNewId] = new Pimple() {
                    Id = pimpleNewId,
                    Position = metaSingula.Transform.InverseTransformPoint(oldPosition),
                    JoinId = pimple.Value.JoinId
                };

                pimpleNewId++;
            }

            var oldPimplePosition = leftSingula.Transform.TransformPoint(
                leftSingula.Pimples[leftCommonPimpleId].Position);

            pimples[pimpleNewId] = new Pimple() {
                Id = pimpleNewId,
                Position = metaSingula.Transform.InverseTransformPoint(oldPimplePosition),
                JoinId = nextJoinId
            };

            return pimples;
        }
    }
}