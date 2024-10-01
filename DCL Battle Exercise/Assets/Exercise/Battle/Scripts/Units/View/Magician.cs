using UnityEngine;

namespace DCLBattle.Battle
{
    // THis is really a copy of Archer class so far
    public sealed class Magician : UnitBase<MagicianModelSO>
    {
        public override UnitType UnitType => UnitType.Magician;

        // using an arrow pool, but we could create a specialized pool for the magician spells
        private static ArcherArrowPool _arrowsPool;

        public override void Initialize(UnitCreationParameters parameters)
        {
            base.Initialize(parameters);

            // TODO this is kind of avoidable I think
            if (_arrowsPool == null)
            {
                _arrowsPool = new ArcherArrowPool(Model.FireBallPrefab, Model.MinFireballPoolSize, Model.MaxFireballPoolSize);
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

            IProjectile fireball = _arrowsPool.RequestPoolableObject();
            fireball.Launch(this, target);

            Animator.SetTrigger("Attack");
            ResetAttackCooldown();
        }

        public void OnDeathAnimFinished()
        {
            Destroy(gameObject);
        }
    }
}