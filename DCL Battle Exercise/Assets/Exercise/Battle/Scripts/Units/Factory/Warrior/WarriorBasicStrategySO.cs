using UnityEngine;

namespace DCLBattle.Battle
{
    [CreateAssetMenu(menuName = "DCLBattle/Units/Factory/Warrior/Strategies/Create Basic Strategy", fileName = "WarriorBasicStrategy", order = 0)]
    public sealed class WarriorBasicStrategySO : StrategySO
    {
        public override ArmyStrategy ArmyStrategy => ArmyStrategy.Basic;

        public override IStrategyUpdater CreateStrategyUpdater()
        {
            return new WarriorBasicStrategyUpdater();
        }
    }
}