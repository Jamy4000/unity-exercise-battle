using System.Collections.Generic;
using UnityEngine;

namespace DCLBattle.Battle
{
    public sealed class CavalryBasicStrategyUpdater : IStrategyUpdater
    {
        public ArmyStrategy ArmyStrategy => ArmyStrategy.Basic;

        public Vector3 UpdateStrategy(UnitBase unitToUpdate)
        {
            UnitBase closestEnemy = unitToUpdate.Army.GetClosestEnemy(unitToUpdate.Position, out float distance);

            unitToUpdate.Attack(closestEnemy);

            // normalizing
            Vector3 toNearest = (closestEnemy.Position - unitToUpdate.Position) / distance;
            toNearest.Scale(IStrategyUpdater.FlatScale);
            return Vector3.Normalize(toNearest);
        }
    }

    public sealed class CavalryDefensiveStrategyUpdater : IStrategyUpdater
    {
        public ArmyStrategy ArmyStrategy => ArmyStrategy.Defensive;

        public Vector3 UpdateStrategy(UnitBase unitToUpdate)
        {
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
                if (distance < unitToUpdate.AttackRange)
                    unitToUpdate.Attack(closestEnemy);
            }
            else
            {
                moveDirection -= toNearestEnemy;
            }

            return Vector3.Normalize(moveDirection);
        }
    }
}