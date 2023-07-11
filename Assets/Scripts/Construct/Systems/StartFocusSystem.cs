using Construct.Components;
using Leopotam.EcsLite;

namespace Construct.Systems
{
    public sealed class StartFocusSystem : IEcsRunSystem
    {
        private readonly EcsWorld _world;
        private readonly EcsFilter _singulaStartFocusFilter;
        private readonly EcsPool<Singula> _singulaPool;
        private readonly EcsPool<StartFocus> _startFocusPool;
        private readonly EcsPool<MetaSingula> _metaSingulaPool;

        public StartFocusSystem(EcsWorld world)
        {
            _world = world;
            _singulaStartFocusFilter = _world.Filter<Singula>().Inc<StartFocus>().End();
            _singulaPool = _world.GetPool<Singula>();
            _startFocusPool = _world.GetPool<StartFocus>();
            _metaSingulaPool = _world.GetPool<MetaSingula>();
        }

        public void Run (IEcsSystems systems)
        {
            foreach (var entity in _singulaStartFocusFilter) {
                if (_metaSingulaPool.Has(entity)) {
                    ref var metaSingula = ref _metaSingulaPool.Get(entity);

                    foreach (var singulaEntity in metaSingula.SingulaEcsEntities) {
                        ref var singula = ref _singulaPool.Get(singulaEntity);
                        singula.Outline.enabled = true;
                    }
                } else {
                    ref var singula = ref _singulaPool.Get(entity);
                    singula.Outline.enabled = true;
                }

                _startFocusPool.Del(entity);
            }
        }
    }
}