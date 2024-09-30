using UnityEngine;

namespace DCLBattle.Battle
{
    public sealed class CavalryBasicStrategyUpdater : IStrategyUpdater
    {
        public ArmyStrategy ArmyStrategy => ArmyStrategy.Basic;

        public Vector3 UpdateStrategy(UnitBase unitToUpdate)
        {
            return Vector3.zero;
        }
    }

    public sealed class CavalryDefensiveStrategyUpdater : IStrategyUpdater
    {
        public ArmyStrategy ArmyStrategy => ArmyStrategy.Defensive;

        public Vector3 UpdateStrategy(UnitBase unitToUpdate)
        {
            return Vector3.zero;
        }
    }
}