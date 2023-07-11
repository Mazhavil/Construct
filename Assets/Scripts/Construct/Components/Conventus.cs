using System.Collections.Generic;
using Construct.Model;

namespace Construct.Components
{
    /// <summary>
    /// Компонент сборки.
    /// </summary>
    public struct Conventus
    {
        /// <summary>
        /// Уникальный идентификатор сборки.
        /// </summary>
        public int Id;

        /// <summary>
        /// Название сборки.
        /// </summary>
        public string Name;

        /// <summary>
        /// Словарь соединений в сборке.
        /// </summary>
        public Dictionary<int, Join> Joins;
    }
}