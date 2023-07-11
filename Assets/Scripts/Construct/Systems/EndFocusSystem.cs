using Construct.Components;
using Leopotam.EcsLite;

namespace Construct.Systems
{
    public sealed class EndFocusSystem : IEcsRunSystem
    {
        private readonly EcsWorld _world;
        private readonly EcsFilter _singulaEndFocusFilter;
        private readonly EcsFilter _metaSingulaEndFocusFilter;
        private readonly EcsPool<Singula> _singulaPool;
        private readonly EcsPool<EndFocus> _endFocusPool;
        private readonly EcsPool<MetaSingula> _metaSingulaPool;

        public EndFocusSystem(EcsWorld world)
        {
            _world = world;
            _singulaEndFocusFilter = _world.Filter<Singula>().Inc<EndFocus>().Exc<MetaSingula>().End();
            _metaSingulaEndFocusFilter = _world.Filter<Singula>().Inc<EndFocus>().Inc<MetaSingula>().End();
            _singulaPool = _world.GetPool<Singula>();
            _endFocusPool = _world.GetPool<EndFocus>();
            _metaSingulaPool = _world.GetPool<MetaSingula>();
        }

        public void Run (IEcsSystems systems)
        {
            // Произвести окончание выделения для всех моделей деталей.
            foreach (var entity in _singulaEndFocusFilter) {
                ref var singula = ref _singulaPool.Get(entity);
                singula.Outline.enabled = false;
                _endFocusPool.Del(entity);
            }

            // Произвести окончание выделения для всех моделей мета деталей.
            foreach (var entity in _metaSingulaEndFocusFilter) {
                ref var metaSingula = ref _metaSingulaPool.Get(entity);

                foreach (var singulaEntity in metaSingula.SingulaEcsEntities) {
                    ref var singula = ref _singulaPool.Get(singulaEntity);
                    singula.Outline.enabled = false;
                }

                _endFocusPool.Del(entity);
            }
        }
    }
}