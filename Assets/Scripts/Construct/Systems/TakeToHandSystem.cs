using System.Collections.Generic;
using System.Linq;
using Construct.Components;
using Construct.Model;
using Leopotam.EcsLite;

namespace Construct.Systems
{
    public sealed class TakeToHandSystem : IEcsRunSystem
    {
        private readonly EcsWorld _world;
        private readonly EcsFilter _takeToHandFilter;
        private readonly EcsFilter _otherSingulaFilter;
        private readonly EcsPool<Singula> _singulaPool;
        private readonly EcsPool<TakeToHand> _takeToHandPool;
        private readonly EcsPool<InHand> _inHandPool;
        private readonly EcsPool<PossibleJoin> _possibleJoinPool;
        private readonly EcsPool<EndFocus> _endFocusPool;
        private readonly EcsPool<Conventus> _conventusPool;

        public TakeToHandSystem(EcsWorld world)
        {
            _world = world;
            _takeToHandFilter = _world.Filter<Singula>().Inc<TakeToHand>().End();
            _otherSingulaFilter = _world.Filter<Singula>().Exc<TakeToHand>().Exc<InJoin>().End();
            _singulaPool = _world.GetPool<Singula>();
            _takeToHandPool = _world.GetPool<TakeToHand>();
            _inHandPool = _world.GetPool<InHand>();
            _possibleJoinPool = _world.GetPool<PossibleJoin>();
            _endFocusPool = _world.GetPool<EndFocus>();
            _conventusPool = _world.GetPool<Conventus>();
        }

        public void Run (IEcsSystems systems)
        {
            foreach (var entity in _takeToHandFilter) {
                ref var singula = ref _singulaPool.Get(entity);
                ref var conventus = ref _conventusPool.Get(singula.ConventusEcsEntity);

                GetNextJoinPairs(singula.Pimples, conventus, out var nextJoinPairs);

                foreach (var otherEntity in _otherSingulaFilter) {
                    ref var otherSingula = ref _singulaPool.Get(otherEntity);
                    GetNextJoinPairs(otherSingula.Pimples, conventus, out var otherNextJoinPairs);
                    var commonNextJoinIds = nextJoinPairs.Keys.Intersect(otherNextJoinPairs.Keys);

                    if (commonNextJoinIds.Count() > 0) {
                        ref var possibleJoin = ref _possibleJoinPool.Add(otherEntity);
                        possibleJoin.PimpleIdSingulaFrame = -1;
                        possibleJoin.SingulaFrame = null;
                        possibleJoin.PimplePairs = commonNextJoinIds.ToDictionary(
                            nextJoinId => otherNextJoinPairs[nextJoinId],
                            nextJoinId => nextJoinPairs[nextJoinId]);
                    }
                }

                _endFocusPool.Add(entity);
                _inHandPool.Add(entity).PossibleJoinEcsEntity = -1;
                _takeToHandPool.Del(entity);
            }
        }

        /// <summary>
        /// Создаёт словарь содержащий уникальный идентификатор следующего <see cref="Join">Join</see> и <see cref="Pimple">Pimple</see>
        /// </summary>
        /// <param name="pimples"></param>
        /// <param name="conventus"></param>
        /// <param name="result"></param>
        private void GetNextJoinPairs(
            in Dictionary<int, Pimple> pimples,
            in Conventus conventus,
            out Dictionary<int, int> result)
        {
            result = new Dictionary<int, int>();

            foreach (var pimple in pimples) {
                foreach (var nextJoinId in conventus.Joins[pimple.Value.JoinId].NextJoinIds) {
                    result[nextJoinId] = pimple.Value.Id;
                }
            }
        }
    }
}