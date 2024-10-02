using System.Collections.Generic;
using UnityEngine;

namespace DCLBattle.Battle
{
    public sealed class ArcherBasicStrategyUpdater : IStrategyUpdater
    {
        public ArmyStrategy ArmyStrategy => ArmyStrategy.Basic;

        public Vector3 UpdateStrategy(UnitBase unitToUpdate)
        {
            UnitBase closestEnemy = unitToUpdate.Army.GetClosestEnemy(unitToUpdate.Position, out float distance);

            // AttackRange check done in the Attack Method
            unitToUpdate.Attack(closestEnemy);

            // normalizing
            Vector3 toNearest = (closestEnemy.Position - unitToUpdate.Position) / distance;
            toNearest.Scale(IStrategyUpdater.FlatScale);
            return Vector3.Normalize(toNearest);
        }
    }

    public sealed class ArcherDefensiveStrategyUpdater : IStrategyUpdater
    {
        public ArmyStrategy ArmyStrategy => ArmyStrategy.Defensive;

        private readonly ArcherDefensiveStrategySO _data;

        private static readonly Quaternion _flankRotation = Quaternion.Euler(0f, 90f, 0f);

        public ArcherDefensiveStrategyUpdater(ArcherDefensiveStrategySO data)
        {
            _data = data;
        }

        public Vector3 UpdateStrategy(UnitBase unitToUpdate)
        {
            Vector3 moveDirection = Vector3.zero;

            // We get the enemies' center point
            List<Army> opponentsArmies = unitToUpdate.Army.GetEnemyArmies();
            Vector3 opponentsArmiesCenter = Vector3.zero;
            for (int i = 0; i < opponentsArmies.Count; i++)
            {
                opponentsArmiesCenter += opponentsArmies[i].Center;
            }
            opponentsArmiesCenter /= opponentsArmies.Count;

            // If we are further away than attack range; we move toward the enemies' center
            // TODO why are we only using X here ?
            float unitPositionX = unitToUpdate.Position.x;
            float distToEnemyX = Mathf.Abs(opponentsArmiesCenter.x - unitPositionX);

            if (distToEnemyX > unitToUpdate.AttackRange)
            {
                if (opponentsArmiesCenter.x < unitPositionX)
                    moveDirection += Vector3.left;
                
                if (opponentsArmiesCenter.x > unitPositionX)
                    moveDirection += Vector3.right;
            }

            // We check who the closest enemy is
            UnitBase closestEnemy = unitToUpdate.Army.GetClosestEnemy(unitToUpdate.Position, out float distance);

            Vector3 toNearest = Vector3.Normalize(closestEnemy.Position - unitToUpdate.Position);
            toNearest.Scale(IStrategyUpdater.FlatScale);

            // if the unit is within attack range
            if (distance < unitToUpdate.AttackRange)
            {
                Vector3 flank = _flankRotation * toNearest;
                moveDirection += Vector3.Normalize(-(toNearest + flank));

                // no need to call attack if the unit is further than attack range
                unitToUpdate.Attack(closestEnemy);
            }
            else
            {
                moveDirection += Vector3.Normalize(toNearest);
            }

            return Vector3.Normalize(moveDirection);
        }
    }
}