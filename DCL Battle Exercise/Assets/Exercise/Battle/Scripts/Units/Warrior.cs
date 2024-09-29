using System.Collections.Generic;
using UnityEngine;

namespace DCLBattle.Battle
{
    public sealed class Warrior : UnitBase, IAttacker
    {
        // TODO Serialize
        private float _attackRange = 2.5f;

        private float _attackRangeSq;

        private float _attackCooldown = 0f;

        public float Damage => throw new System.NotImplementedException();
        public float MaxAttackCooldown => throw new System.NotImplementedException();
        public float PostAttackDelay => throw new System.NotImplementedException();

        protected override void Awake()
        {
            base.Awake();
            _attackRangeSq = _attackRange * _attackRange;
        }

        public void Attack(IAttackReceiver target)
        {
            if (_attackCooldown > 0)
                return;

            if (Vector3.SqrMagnitude(transform.position - target.Position) > _attackRangeSq)
                return;

            _attackCooldown = MaxAttackCooldown;

            Animator.SetTrigger("Attack");

            target.Hit(this, target.Position, Damage);
        }

        public void OnDeathAnimFinished()
        {
            // TODO Pooling
            Destroy(gameObject);
        }


        protected override void UpdateDefensive(List<UnitBase> allies, List<UnitBase> enemies)
        {
            /*
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
            */
        }

        protected override void UpdateBasic(List<UnitBase> allies, List<UnitBase> enemies)
        {
            /*
            DCLBattleUtils.GetNearestObject(this, enemies, out UnitBase nearestEnemy);

            if (nearestEnemy == null)
                return;

            Vector3 toNearest = (nearestEnemy.transform.position - transform.position).normalized;
            toNearest.Scale(new Vector3(1, 0, 1));
            Move(toNearest.normalized);

            Attack(nearestEnemy);
            */
        }
    }
}