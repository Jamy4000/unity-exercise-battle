using System.Collections.Generic;
using UnityEngine;

namespace DCLBattle.Battle
{
    public sealed class ArcherBasicStrategyUpdater : IStrategyUpdater
    {
        public ArmyStrategy ArmyStrategy => ArmyStrategy.Basic;

        public Vector3 UpdateStrategy(UnitBase unitToUpdate)
        {
            UnitBase closestEnemy = unitToUpdate.Army.GetClosestEnemy(unitToUpdate.Position, out _);

            // if they are no more unit to attack, this army won.
            // TODO This check isn't really necessary as systems should stop as soon as there ar no more unit to fight
            if (closestEnemy == null)
                return Vector3.zero;

            // TODO I don't think this should be here
            unitToUpdate.Attack(closestEnemy);

            Vector3 toNearest = Vector3.Normalize(closestEnemy.Position - unitToUpdate.Position);
            toNearest.Scale(IStrategyUpdater.FlatScale);
            return toNearest;
        }
    }

    public sealed class ArcherDefensiveStrategyUpdater : IStrategyUpdater
    {
        public ArmyStrategy ArmyStrategy => ArmyStrategy.Defensive;

        private readonly ArcherDefensiveStrategySO _data;

        public ArcherDefensiveStrategyUpdater(ArcherDefensiveStrategySO data)
        {
            _data = data;
        }

        public Vector3 UpdateStrategy(UnitBase unitToUpdate)
        {
            Vector3 moveDirection = Vector3.zero;

            // We get the enemies' center point
            List<Army> enemies = unitToUpdate.Army.GetEnemyArmies();
            Vector3 enemiesCenter = Vector3.zero;
            for (int i = 0; i < enemies.Count; i++)
            {
                enemiesCenter += enemies[i].Center;
            }
            enemiesCenter /= enemies.Count;

            // If we are further away than attack range; we move toward the enemies' center
            // TODO why are we only using X here ?
            float unitPositionX = unitToUpdate.Position.x;
            float distToEnemyX = Mathf.Abs(enemiesCenter.x - unitPositionX);
            if (distToEnemyX > unitToUpdate.AttackRange)
            {
                if (enemiesCenter.x < unitPositionX)
                    moveDirection += Vector3.left;
                else if (enemiesCenter.x > unitPositionX)
                    moveDirection += Vector3.right;
            }

            // We check who the closest enemy is
            UnitBase closestEnemy = unitToUpdate.Army.GetClosestEnemy(unitToUpdate.Position, out float distance);
            // TODO This should never happen
            if (closestEnemy == null)
                return moveDirection;

            Vector3 toNearest = Vector3.Normalize(closestEnemy.Position - unitToUpdate.Position);
            toNearest.Scale(IStrategyUpdater.FlatScale);

            // if the unit is within attack range
            if (distance < unitToUpdate.AttackRange)
            {
                Vector3 flank = Quaternion.Euler(0f, 90f, 0f) * toNearest;
                moveDirection += Vector3.Normalize(-(toNearest + flank));

                // Slight change in design, we were always attacking the closest enemy, even when not in range
                unitToUpdate.Attack(closestEnemy);
            }
            else
            {
                moveDirection += Vector3.Normalize(toNearest);
            }

            return moveDirection;
        }
    }
}