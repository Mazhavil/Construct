namespace Construct.Components
{
    /// <summary>
    /// Компонент указывающий что деталь находтся в руке.
    /// </summary>
    public struct InHand
    {
        /// <summary>
        /// Сущность детали, с которой можно произвести соединение.
        /// </summary>
        public int PossibleJoinEcsEntity;
    }
}