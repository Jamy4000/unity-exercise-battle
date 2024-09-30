using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCLBattle.Battle
{
    public sealed class ArcherBasicStrategyUpdater : IStrategyUpdater
    {
        public ArmyStrategy ArmyStrategy => ArmyStrategy.Basic;

        public void UpdateStrategy(IUnit unitToUpdate)
        {
            List<Army> enemyArmies = unitToUpdate.Army.GetEnemyArmies();

            (IUnit unit, float distance) closestUnit = new (null, Mathf.Infinity);
            for (int armyIndex = 0; armyIndex < enemyArmies.Count; armyIndex++)
            {
                IUnit enemyUnit = enemyArmies[armyIndex].GetClosestUnit(unitToUpdate.Position, out float enemyDistance);
                // no unit found
                if (enemyUnit == null)
                    continue;

                if (closestUnit.unit == null || enemyDistance < closestUnit.distance)
                    closestUnit = (enemyUnit, enemyDistance);
            }

            // if they are no more unit to attack, this army won.
            // TODO This check isn't really necessary as systems should stop as soon as 
            if (closestUnit.unit == null)
                return;

            Vector3 toNearest = Vector3.Normalize(closestUnit.unit.Position - unitToUpdate.Position);
            toNearest.Scale(IStrategyUpdater.FlatScale);
            unitToUpdate.Move(toNearest.normalized);

            // TODO this is bad
            unitToUpdate.Attack(closestUnit.unit as IAttackReceiver);
        }
    }

    public sealed class ArcherDefensiveStrategyUpdater : IStrategyUpdater
    {
        public ArmyStrategy ArmyStrategy => ArmyStrategy.Defensive;

        public void UpdateStrategy(IUnit unitToUpdate)
        {
            /*
            Vector3 enemyCenter = DCLBattleUtils.GetCenter(enemies);
            float distToEnemyX = Mathf.Abs(enemyCenter.x - transform.position.x);

            if (distToEnemyX > _attackRange)
            {
                if (enemyCenter.x < transform.position.x)
                    Move(Vector3.left);

                if (enemyCenter.x > transform.position.x)
                    Move(Vector3.right);
            }

            float distToNearest = DCLBattleUtils.GetNearestObject(this, enemies, out UnitBase nearestEnemy);

            if (nearestEnemy == null)
                return;

            if (distToNearest < _attackRange)
            {
                Vector3 toNearest = (nearestEnemy.transform.position - transform.position).normalized;
                toNearest.Scale(_flatScale);

                Vector3 flank = Quaternion.Euler(0f, 90f, 0f) * toNearest;
                Move(-(toNearest + flank).normalized);
            }
            else
            {
                Vector3 toNearest = (nearestEnemy.transform.position - transform.position).normalized;
                toNearest.Scale(_flatScale);
                Move(toNearest.normalized);
            }

            Attack(nearestEnemy);
            */
        }
    }
}