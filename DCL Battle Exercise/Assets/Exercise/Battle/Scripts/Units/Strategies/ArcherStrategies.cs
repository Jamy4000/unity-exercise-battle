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
            List<IArmy> enemyArmies = unitToUpdate.Army.GetEnemyArmies();
            (IUnit, float)[] closestUnits = new (IUnit, float)[enemyArmies.Count];
            for (int armyIndex = 0; armyIndex < enemyArmies.Count; armyIndex++)
            {
                IUnit unit = enemyArmies[armyIndex].GetClosestUnit(unitToUpdate.Position, out float distance);
                closestUnits[armyIndex] = (unit, distance);
            }

            (IUnit unit, float distance) closestUnit = (null, Mathf.Infinity);
            foreach ((IUnit unit, float distance) unitDistancePair in closestUnits)
            {
                if (unitDistancePair.unit == null)
                    continue;

                if (closestUnit.unit == null || unitDistancePair.distance < closestUnit.distance)
                    closestUnit = unitDistancePair;
            }

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