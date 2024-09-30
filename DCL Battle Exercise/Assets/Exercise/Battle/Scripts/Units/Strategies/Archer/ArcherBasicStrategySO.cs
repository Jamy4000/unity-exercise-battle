using UnityEngine;

namespace DCLBattle.Battle
{
    [CreateAssetMenu(menuName = "DCLBattle/Units/Factory/Archer/Strategies/Create Basic Strategy", fileName = "ArcherBasicStrategy", order = 0)]
    public sealed class ArcherBasicStrategySO : StrategySO
    {
        public override ArmyStrategy ArmyStrategy => ArmyStrategy.Basic;

        public override IStrategyUpdater CreateStrategyUpdater()
        {
            return new ArcherBasicStrategyUpdater();
        }
    }
}