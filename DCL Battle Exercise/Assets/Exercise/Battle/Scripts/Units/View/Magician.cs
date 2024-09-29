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


        private float _attackCooldown = 0f;

        public float Damage => throw new System.NotImplementedException();
        public float MaxAttackCooldown => throw new System.NotImplementedException();
        public float PostAttackDelay => throw new System.NotImplementedException();

        public override UnitType UnitType => UnitType.Magician;

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