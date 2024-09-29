using UnityEngine;

namespace DCLBattle.Battle
{
    public sealed class CavalryBasicStrategyUpdater : IStrategyUpdater
    {
        public ArmyStrategy ArmyStrategy => ArmyStrategy.Basic;

        public void UpdateStrategy(IUnit unitToUpdate)
        {
            DCLBattleUtils.GetNearestObject(this, enemies, out UnitBase nearestEnemy);

            if (nearestEnemy == null)
                return;

            Vector3 toNearest = (nearestEnemy.transform.position - transform.position).normalized;
            toNearest.Scale(new Vector3(1, 0, 1));
            Move(toNearest.normalized);

            Attack(nearestEnemy);
        }
    }

    public sealed class CavalryDefensiveStrategyUpdater : IStrategyUpdater
    {
        public ArmyStrategy ArmyStrategy => ArmyStrategy.Defensive;

        public void UpdateStrategy(IUnit unitToUpdate)
        {
            Vector3 enemyCenter = DCLBattleUtils.GetCenter(enemies);

            // TODO Hard coded value
            if (Mathf.Abs(enemyCenter.x - transform.position.x) > 20f)
            {
                if (enemyCenter.x < transform.position.x)
                    Move(Vector3.left);

                if (enemyCenter.x > transform.position.x)
                    Move(Vector3.right);
            }

            DCLBattleUtils.GetNearestObject(this, enemies, out UnitBase nearestObject);

            if (nearestObject == null)
                return;

            if (_attackCooldown <= 0)
            {
                Move((nearestObject.transform.position - transform.position).normalized);
            }
            else
            {
                Move((nearestObject.transform.position - transform.position).normalized * -1);
            }

            Attack(nearestObject);
        }
    }
}