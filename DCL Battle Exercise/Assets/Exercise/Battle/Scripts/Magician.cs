using System.Collections.Generic;
using UnityEngine;

namespace DCLBattle.Battle
{
    // THis is really a copy of Archer class so far
    public sealed class Magician : UnitBase, IAttacker
    {
        // TODO add this to SO ?
        [SerializeField]
        private float _attackRange = 20f;
        private float _attackRangeSq;

        [SerializeField, Interface(typeof(IProjectile))]
        private Object _arrowPrefab;

        private Color _color;
        public Color ArmyColor => _color;

        public int ArmyID => throw new System.NotImplementedException();

        public override UnitType UnitType => UnitType.Magician;

        private static readonly Vector3 _flatScale = new Vector3(1f, 0f, 1f);

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

        private void Start()
        {
            // TODO not a fan of that one
            _color = GetComponentInChildren<Renderer>().material.color;
        }

        public void Attack(IAttackReceiver target)
        {
            if (attackCooldown > 0)
                return;

            if (Vector3.SqrMagnitude(transform.position - target.Position) > _attackRangeSq)
                return;

            // TODO Pooling
            IProjectile projectile = Instantiate(_arrowPrefab) as IProjectile;
            projectile.Launch(this, target);

            Animator.SetTrigger("Attack");
            attackCooldown = maxAttackCooldown;
        }

        public void OnDeathAnimFinished()
        {
            Destroy(gameObject);
        }

        protected override void UpdateDefensive(List<UnitBase> allies, List<UnitBase> enemies)
        {
            Vector3 enemyCenter = Utils.GetCenter(enemies);
            float distToEnemyX = Mathf.Abs(enemyCenter.x - transform.position.x);

            if (distToEnemyX > _attackRange)
            {
                if (enemyCenter.x < transform.position.x)
                    Move(Vector3.left);

                if (enemyCenter.x > transform.position.x)
                    Move(Vector3.right);
            }

            float distToNearest = Utils.GetNearestObject(this, enemies, out UnitBase nearestEnemy);

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
            Utils.GetNearestObject(this, enemies, out UnitBase nearestEnemy);

            if (nearestEnemy == null)
                return;

            Vector3 toNearest = (nearestEnemy.transform.position - transform.position).normalized;
            toNearest.Scale(_flatScale);
            Move(toNearest.normalized);

            Attack(nearestEnemy);
        }
    }
}