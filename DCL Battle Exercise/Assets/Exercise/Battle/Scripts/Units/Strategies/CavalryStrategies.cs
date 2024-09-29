using UnityEngine;

namespace DCLBattle.Battle
{
    public sealed class CavalryBasicStrategyUpdater : IStrategyUpdater
    {
        public ArmyStrategy ArmyStrategy => ArmyStrategy.Basic;

        public void UpdateStrategy(IUnit unitToUpdate)
        {
        }
    }

    public sealed class CavalryDefensiveStrategyUpdater : IStrategyUpdater
    {
        public ArmyStrategy ArmyStrategy => ArmyStrategy.Defensive;

        public void UpdateStrategy(IUnit unitToUpdate)
        {
        }
    }
}