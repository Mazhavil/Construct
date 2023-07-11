using Construct.Components;
using Leopotam.EcsLite;

namespace Construct
{
    sealed class DeleteSingulaSystem : IEcsRunSystem
    {
        private readonly EcsWorld _world;
        private EcsFilter _filter;

        public DeleteSingulaSystem(EcsWorld world)
        {
            _world = world;
            _filter = _world.Filter<Singula>().Inc<DeleteSingula>().End();
        }

        public void Run (IEcsSystems systems) {
            foreach (var entity in _filter) {
                _world.DelEntity(entity);
            }
        }
    }
}