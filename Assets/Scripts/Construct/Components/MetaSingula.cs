using System.Collections.Generic;
using Construct.Views;

namespace Construct.Components
{
    /// <summary>
    /// Компонент мета-детали, объединяющей несколько деталей.
    /// </summary>
    public struct MetaSingula
    {
        /// <summary>
        /// Ссылка на MetaSingulaView.
        /// </summary>
        public MetaSingulaView MetaSingulaView;

        /// <summary>
        /// Словарь деталей, которые содержит мета-деталь.
        /// Key - Уникальный идентификатор точки соединения.
        /// Value - Массив уникальных идентификаторов деталей, сединённых в точке.
        /// </summary>
        public Dictionary<int, List<int>> PimpleSingulaEcsEntities;

        public List<int> SingulaEcsEntities;
    }
}
