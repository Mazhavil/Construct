using System.Collections.Generic;
using Construct.Model;
using UnityEngine;

namespace Construct.Components
{
    /// <summary>
    /// Компонент содержащий информацию возможных соединениях.
    /// Может содержаться только сущности детали, к которой можно присоединить деталь,
    /// нахадящуюся в руках пользователя.
    /// </summary>
    public struct PossibleJoin
    {
        /// <summary>
        /// Словарь содержащий уникальные идентификаторы <see cref="Pimple" />, которые можно соединить.
        /// Key - Уникальный идентификатор <see cref="Pimple" /> текущей детали.
        /// Value - Уникальный идентификатор <see cref="Pimple" /> детали в руке пользователя.
        /// </summary>
        public Dictionary<int, int> PimplePairs;

        /// <summary>
        /// Уникальный идентификатор <see cref="Pimple"/> к которой прикреплена вспомогательная модель.
        /// </summary>
        public int PimpleIdSingulaFrame;

        /// <summary>
        /// Ссылка на вспомогательную модель.
        /// </summary>
        public GameObject SingulaFrame;
    }
}