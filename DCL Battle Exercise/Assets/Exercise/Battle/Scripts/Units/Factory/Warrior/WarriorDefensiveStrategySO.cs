using UnityEngine;

namespace DCLBattle.Battle
{
    [CreateAssetMenu(menuName = "DCLBattle/Units/Factory/Warrior/Strategies/Create Defensive Strategy", fileName = "WarriorDefensiveStrategy", order = 0)]
    public sealed class WarriorDefensiveStrategySO : StrategySO
    {
        public override ArmyStrategy ArmyStrategy => ArmyStrategy.Defensive;

        public override IStrategyUpdater CreateStrategyUpdater()
        {
            return new WarriorDefensiveStrategyUpdater();
        }
    }
}