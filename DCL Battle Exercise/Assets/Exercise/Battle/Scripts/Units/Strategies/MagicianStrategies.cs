using UnityEngine;

namespace DCLBattle.Battle
{
    public sealed class MagicianBasicStrategyUpdater : IStrategyUpdater
    {
        public ArmyStrategy ArmyStrategy => ArmyStrategy.Basic;

        public void UpdateStrategy(UnitBase unitToUpdate)
        {
        }
    }

    public sealed class MagicianDefensiveStrategyUpdater : IStrategyUpdater
    {
        public ArmyStrategy ArmyStrategy => ArmyStrategy.Defensive;

        public void UpdateStrategy(UnitBase unitToUpdate)
        {
        }
    }
}