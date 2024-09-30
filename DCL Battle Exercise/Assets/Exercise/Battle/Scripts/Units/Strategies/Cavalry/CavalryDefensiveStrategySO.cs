using UnityEngine;

namespace DCLBattle.Battle
{
    [CreateAssetMenu(menuName = "DCLBattle/Units/Factory/Cavalry/Strategies/Create Defensive Strategy", fileName = "CavalryDefensiveStrategy", order = 0)]
    public sealed class CavalryDefensiveStrategySO : StrategySO
    {
        public override ArmyStrategy ArmyStrategy => ArmyStrategy.Defensive;

        public override IStrategyUpdater CreateStrategyUpdater()
        {
            return new CavalryDefensiveStrategyUpdater();
        }
    }
}