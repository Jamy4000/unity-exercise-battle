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

        public override UnitType UnitType => UnitType.Warrior;

        protected override void Awake()
        {
            base.Awake();
            _attackRangeSq = _attackRange * _attackRange;
        }

        public override void Attack(IAttackReceiver target)
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
    }
}