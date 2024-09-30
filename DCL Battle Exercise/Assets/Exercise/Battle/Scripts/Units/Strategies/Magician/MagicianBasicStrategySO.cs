using UnityEngine;

namespace DCLBattle.Battle
{
    [CreateAssetMenu(menuName = "DCLBattle/Units/Factory/Magician/Strategies/Create Basic Strategy", fileName = "MagicianBasicStrategy", order = 0)]
    public sealed class MagicianBasicStrategySO : StrategySO
    {
        public override ArmyStrategy ArmyStrategy => ArmyStrategy.Basic;

        public override IStrategyUpdater CreateStrategyUpdater()
        {
            return new MagicianBasicStrategyUpdater();
        }
    }
}