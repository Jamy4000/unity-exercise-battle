using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace DCLBattle.Battle
{
    public sealed class CavalryBasicStrategyUpdater : IStrategyUpdater, I_Startable
    {
        public ArmyStrategy ArmyStrategy => ArmyStrategy.Basic;

        public bool HasStarted { get; set; }

        private IArmiesHolder _armiesHolder;

        public CavalryBasicStrategyUpdater()
        {
            GameUpdater.Register(this);
        }

        public void Start()
        {
            _armiesHolder = UnityServiceLocator.ServiceLocator.Global.Get<IArmiesHolder>();
            GameUpdater.Unregister(this);
        }

        public TargetInfo UpdateStrategy(UnitData unitData, out Vector3 strategyMovement)
        {
            var unitArmy = _armiesHolder.GetArmy(unitData.ArmyID);

            TargetInfo target = unitArmy.GetClosestEnemy(unitData.Position, out float distance);

            // normalizing
            strategyMovement = (target.Position - unitData.Position) / distance;
            strategyMovement.Scale(IStrategyUpdater.FlatScale);
            return target;
        }
    }

    public sealed class CavalryDefensiveStrategyUpdater : IStrategyUpdater
    {
        public ArmyStrategy ArmyStrategy => ArmyStrategy.Defensive;

        public TargetInfo UpdateStrategy(UnitData unitData, out Vector3 strategyMovement)
        {
            strategyMovement = Vector3.zero;
            return default;
            /*
            Vector3 moveDirection = Vector3.zero;

            // We get the enemies' center point
            Vector3 enemyCenter = Vector3.zero;

            List<Army> opponentsArmies = unitToUpdate.Army.GetEnemyArmies();
            for (int i = 0; i < opponentsArmies.Count; i++)
            {
                enemyCenter += opponentsArmies[i].Center;
            }
            enemyCenter /= opponentsArmies.Count;

            // If we are further away than attack range; we move toward the enemies' center
            // TODO why are we only using X here ?
            float unitPositionX = unitToUpdate.Position.x;
            float distToEnemyX = Mathf.Abs(enemyCenter.x - unitPositionX);

            // TODO Hard coded value
            if (distToEnemyX > 20f)
            {
                if (enemyCenter.x < unitPositionX)
                    moveDirection += Vector3.left;

                else if (enemyCenter.x > unitPositionX)
                    moveDirection += Vector3.right;
            }

            // We check who the closest enemy is
            UnitBase closestEnemy = unitToUpdate.Army.GetClosestEnemy(unitToUpdate.Position, out float distance);

            Vector3 toNearestEnemy = Vector3.Normalize(closestEnemy.Position - unitToUpdate.Position);
            if (unitToUpdate.AttackCooldown <= 0f)
            {
                moveDirection += toNearestEnemy;

                // AttackRange check done in the Attack Method
                unitToUpdate.Target = closestEnemy;
            }
            else
            {
                moveDirection -= toNearestEnemy;
            }

            return Vector3.Normalize(moveDirection);
            */
        }
    }
}