using UnityEngine;

namespace DCLBattle.Battle
{
    [CreateAssetMenu(menuName = "DCLBattle/Units/Factory/Archer/Strategies/Create Defensive Strategy", fileName = "ArcherDefensiveStrategy", order = 0)]
    public sealed class ArcherDefensiveStrategySO : StrategySO
    {
        public override ArmyStrategy ArmyStrategy => ArmyStrategy.Defensive;

        public override IStrategyUpdater CreateStrategyUpdater()
        {
            return new ArcherDefensiveStrategyUpdater(this);
        }
    }
}