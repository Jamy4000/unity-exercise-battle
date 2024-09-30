using UnityEngine;

namespace DCLBattle.Battle
{
    // THis is really a copy of Archer class so far
    public sealed class Magician : UnitBase
    {
        // TODO add this to SO ?
        [SerializeField]
        private float _attackRange = 20f;
        private float _attackRangeSq;

        [SerializeField]
        private float _maxAttackCooldown = 2f;

        [SerializeField]
        private float _postAttackDelay = 0.5f;

        [SerializeField, Interface(typeof(IProjectile))]
        private Object _arrowPrefab;

        public override UnitType UnitType => UnitType.Magician;

        protected override void Awake()
        {
            base.Awake();
            _attackRangeSq = _attackRange * _attackRange;
        }

        public override void Move(Vector3 delta)
        {
            // TODO We could avoid a bunch of calculations if we were to check this before updating the strategy and evade plan
            if (AttackCooldown > _maxAttackCooldown - _postAttackDelay)
                return;

            base.Move(delta);
        }

        public override void Attack(IAttackReceiver target)
        {
            if (AttackCooldown > 0f)
                return;

            if (Vector3.SqrMagnitude(transform.position - target.Position) > _attackRangeSq)
                return;

            // TODO Pooling
            IProjectile projectile = Instantiate(_arrowPrefab) as IProjectile;
            projectile.Launch(this, target);

            Animator.SetTrigger("Attack");
            ResetAttackCooldown();
        }

        public void OnDeathAnimFinished()
        {
            Destroy(gameObject);
        }
    }
}