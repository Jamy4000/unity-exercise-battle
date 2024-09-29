namespace DCLBattle.Battle
{
    public sealed class ArcherBasicStrategyUpdater : IStrategyUpdater
    {
        public ArmyStrategy ArmyStrategy => ArmyStrategy.Basic;

        public void UpdateStrategy(IUnit unitToUpdate)
        {

        }
    }

    public sealed class ArcherDefensiveStrategyUpdater : IStrategyUpdater
    {
        public ArmyStrategy ArmyStrategy => ArmyStrategy.Defensive;

        public void UpdateStrategy(IUnit unitToUpdate)
        {

        }
    }
}