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
        

        private static readonly Vector3 _flatScale = new Vector3(1f, 0f, 1f);

        public override UnitType UnitType => UnitType.Archer;

        protected override void Awake()
        {
            base.Awake();
            _attackRangeSq = _attackRange * _attackRange;

            // TODO Move this to a SO
            health = 5;
            defense = 0;
            attack = 10;
            maxAttackCooldown = 5f;
            postAttackDelay = 1f;
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
            _attackCooldown = maxAttackCooldown;
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