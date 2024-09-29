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

        // TODO
        public float Damage => 5f;
        public float MaxAttackCooldown => 0.5f;
        public float PostAttackDelay => 1f;

        public override UnitType UnitType => UnitType.Archer;

        protected override void Awake()
        {
            base.Awake();
            _attackRangeSq = _attackRange * _attackRange;
        }

        public override void Move(Vector3 delta)
        {
            _attackCooldown -= Time.deltaTime;
            // TODO This shouldn't be in UnitBase
            if (_attackCooldown > MaxAttackCooldown - PostAttackDelay)
                return;

            base.Move(delta);
        }

        public override void Attack(IAttackReceiver target)
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