using Construct.Components;
using Leopotam.EcsLite;

namespace Construct.Systems
{
    public sealed class DetachSingulaSystem : IEcsRunSystem
    {
        private readonly EcsWorld _world;
        private readonly EcsFilter _singulaDetachFilter;
        private readonly EcsPool<Singula> _singulaPool;
        private readonly EcsPool<DetachSingula> _detachSingulaPool;

        public DetachSingulaSystem(EcsWorld world)
        {
            _world = world;
            _singulaDetachFilter = _world.Filter<DetachSingula>().Inc<Singula>().End();
            _singulaPool = _world.GetPool<Singula>();
            _detachSingulaPool = _world.GetPool<DetachSingula>();
        }

        public void Run (IEcsSystems systems)
        {
            foreach (var entity in _singulaDetachFilter) {
                ref var singula = ref _singulaPool.Get(entity);

                // if (singula.MasterSingulaEcsEntity.HasValue) {
                //     ref var masterSingula = ref _singulaPool.Get(singula.MasterSingulaEcsEntity.Value);

                //     masterSingula.SingulaView.GetComponent<FixedJoint>().connectedBody = null;
                //     singula.SingulaView.transform.SetParent(null);

                //     for (int i = 0; i < masterSingula.SlaveSingulaEcsEntities.Length; i++) {
                //         if (entity == masterSingula.SlaveSingulaEcsEntities[i]) {
                //             masterSingula.SlaveSingulaEcsEntities[i] = -1;
                //             break;
                //         }
                //     }
                // }

                _detachSingulaPool.Del(entity);
            }
        }
    }
}