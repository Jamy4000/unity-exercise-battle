namespace DCLBattle.Battle
{
    public interface IStrategyUpdater
    {
        public static readonly int StrategyCount = System.Enum.GetValues(typeof(ArmyStrategy)).Length;

        ArmyStrategy ArmyStrategy { get; }

        void UpdateStrategy(IUnit unitToUpdate);
    }
}