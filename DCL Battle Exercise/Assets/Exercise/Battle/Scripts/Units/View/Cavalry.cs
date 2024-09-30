using System.Collections.Generic;
using UnityEngine;

namespace DCLBattle.Battle
{
    // copy of warrior, just for the sake of argument
    public sealed class Cavalry : UnitBase<CavalryModelSO>
    {
        [SerializeField]
        private float _attackRange = 2.5f;
        private float _attackRangeSq;

        public override UnitType UnitType => UnitType.Cavalry;

        protected override void Awake()
        {
            base.Awake();
            _attackRangeSq = _attackRange * _attackRange;
        }

        public override void Attack(IAttackReceiver target)
        {
            if (AttackCooldown > 0)
                return;

            if (Vector3.SqrMagnitude(transform.position - target.Position) > _attackRangeSq)
                return;

            Animator.SetTrigger("Attack");

            target.Hit(this, target.Position, Model.Damage);
            ResetAttackCooldown();
        }

        public void OnDeathAnimFinished()
        {
            // TODO Pooling
            Destroy(gameObject);
        }
    }
}