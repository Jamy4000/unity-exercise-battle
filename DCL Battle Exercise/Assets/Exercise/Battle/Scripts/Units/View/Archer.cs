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
    }
}