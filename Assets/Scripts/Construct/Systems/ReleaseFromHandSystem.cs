using Construct.Components;
using Leopotam.EcsLite;
using UnityEngine;

namespace Construct.Systems
{
    public sealed class ReleaseFromHandSystem : IEcsRunSystem
    {
        private readonly EcsWorld _world;
        private readonly EcsFilter _releaseFromHandFilter;
        private readonly EcsFilter _possibleJoinFilter;
        private readonly EcsPool<ReleaseFromHand> _releaseFromHandPool;
        private readonly EcsPool<InHand> _inHandPool;
        private readonly EcsPool<PossibleJoin> _possibleJoinPool;

        public ReleaseFromHandSystem(EcsWorld world)
        {
            _world = world;
            _releaseFromHandFilter = _world.Filter<Singula>().Inc<ReleaseFromHand>().End();
            _possibleJoinFilter = _world.Filter<Singula>().Inc<PossibleJoin>().End();
            _releaseFromHandPool = _world.GetPool<ReleaseFromHand>();
            _possibleJoinPool = _world.GetPool<PossibleJoin>();
            _inHandPool = _world.GetPool<InHand>();
        }

        public void Run (IEcsSystems systems)
        {
            foreach (var entity in _releaseFromHandFilter) {
                foreach (var possibleJoinEntity in _possibleJoinFilter) {
                    GameObject.Destroy(_possibleJoinPool.Get(possibleJoinEntity).SingulaFrame);
                    _possibleJoinPool.Del(possibleJoinEntity);
                }

                _inHandPool.Del(entity);
                _releaseFromHandPool.Del(entity);
            }
        }
    }
}