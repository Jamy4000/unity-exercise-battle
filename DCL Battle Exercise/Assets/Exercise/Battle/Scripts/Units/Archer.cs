using System.Collections.Generic;
using UnityEngine;

namespace DCLBattle.Battle
{
    public sealed class Archer : UnitBase, IAttacker
    {
        [SerializeField]
        private float _attackRange = 20f;
        private float _attackRangeSq;

        [SerializeField, Interface(typeof(IProjectile))]
        private Object _arrowPrefab;
        

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

            // TODO Pooling
            IProjectile projectile = Instantiate(_arrowPrefab) as IProjectile;
            projectile.Launch(this, target);

            Animator.SetTrigger("Attack");
            _attackCooldown = MaxAttackCooldown;
        }

        public void OnDeathAnimFinished()
        {
            Destroy(gameObject);
        }

        protected override void UpdateDefensive(List<UnitBase> allies, List<UnitBase> enemies)
        {
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
        }

        protected override void UpdateBasic(List<UnitBase> allies, List<UnitBase> enemies)
        {
            DCLBattleUtils.GetNearestObject(this, enemies, out UnitBase nearestEnemy);

            if (nearestEnemy == null)
                return;

            Vector3 toNearest = (nearestEnemy.transform.position - transform.position).normalized;
            toNearest.Scale(_flatScale);
            Move(toNearest.normalized);

            Attack(nearestEnemy);
        }
    }
}