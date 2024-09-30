using UnityEngine;

namespace DCLBattle.Battle
{
    public sealed class Warrior : UnitBase, IAttacker
    {
        [SerializeField]
        private float _attackRange = 2.5f;
        private float _attackRangeSq;

        [SerializeField]
        private float _damage = 5f;

        public override UnitType UnitType => UnitType.Warrior;

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

            target.Hit(this, target.Position, _damage);
            ResetAttackCooldown();
        }

        public void OnDeathAnimFinished()
        {
            // TODO Pooling
            Destroy(gameObject);
        }
    }
}