using UnityEngine;

namespace DCLBattle.Battle
{
    [CreateAssetMenu(menuName = "DCLBattle/Units/Factory/Cavalry/Strategies/Create Basic Strategy", fileName = "CavalryBasicStrategy", order = 0)]
    public sealed class CavalryBasicStrategySO : StrategySO
    {
        public override ArmyStrategy ArmyStrategy => ArmyStrategy.Basic;

        public override IStrategyUpdater CreateStrategyUpdater()
        {
            return new CavalryBasicStrategyUpdater();
        }
    }
}