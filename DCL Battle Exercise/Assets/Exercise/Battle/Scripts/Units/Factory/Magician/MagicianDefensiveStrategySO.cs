using UnityEngine;

namespace DCLBattle.Battle
{
    [CreateAssetMenu(menuName = "DCLBattle/Units/Factory/Magician/Strategies/Create Defensive Strategy", fileName = "MagicianDefensiveStrategy", order = 0)]
    public sealed class MagicianDefensiveStrategySO : StrategySO
    {
        public override ArmyStrategy ArmyStrategy => ArmyStrategy.Defensive;

        public override IStrategyUpdater CreateStrategyUpdater()
        {
            return new MagicianDefensiveStrategyUpdater();
        }
    }
}