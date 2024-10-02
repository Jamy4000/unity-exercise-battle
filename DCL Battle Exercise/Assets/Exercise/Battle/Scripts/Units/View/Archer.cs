using UnityEngine;

namespace DCLBattle.Battle
{
    public sealed class Archer : UnitBase<ArcherModelSO>
    {
        public override UnitType UnitType => UnitType.Archer;

        public override void ApplyCalculatedData(UnitData dataSet, TargetInfo targetInfo, IArmiesHolder armiesHolder)
        {
            if (AttackCooldown > Model.MaxAttackCooldown - Model.PostAttackDelay)
            {
                CurrentAttackCooldown -= Time.deltaTime;
            }
            else
            {
                base.ApplyCalculatedData(dataSet, targetInfo, armiesHolder);
            }
        }

        public override void Attack(IAttackReceiver target)
        {
            if (AttackCooldown > 0f)
                return;

            if (Vector3.SqrMagnitude(Position - target.Position) > Model.AttackRangeSq)
                return;

            IProjectile projectile = Model.ArrowPool.RequestPoolableObject();
            projectile.Launch(this, target);

            Animator.SetTrigger("Attack");
            ResetAttackCooldown();
        }
    }
}