using UnityEngine;

namespace DCLBattle.Battle
{
    public sealed class Archer : UnitBase<ArcherModelSO>
    {
        public override UnitType UnitType => UnitType.Archer;

        private static ArcherArrowPool _arrowsPool;

        public override void Initialize(UnitCreationParameters parameters)
        {
            base.Initialize(parameters);
            
            // TODO this should be avoidable I think
            if (_arrowsPool == null)
            {
                _arrowsPool = new ArcherArrowPool(Model.ArrowPrefab, Model.MinArrowPoolSize, Model.MaxArrowPoolSize);
            }
        }
        public override void ManualUpdate()
        {
            if (AttackCooldown > Model.MaxAttackCooldown - Model.PostAttackDelay)
            {
                AttackCooldown -= Time.deltaTime;
            }
            else
            {
                base.ManualUpdate();
            }
        }

        public override void Attack(IAttackReceiver target)
        {
            if (AttackCooldown > 0f)
                return;

            if (Vector3.SqrMagnitude(transform.position - target.Position) > Model.AttackRangeSq)
                return;

            IProjectile projectile = _arrowsPool.RequestPoolableObject();
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