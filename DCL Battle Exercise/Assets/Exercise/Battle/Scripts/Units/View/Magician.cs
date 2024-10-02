using UnityEngine;

namespace DCLBattle.Battle
{
    // THis is really a copy of Archer class so far
    public sealed class Magician : UnitBase<MagicianModelSO>
    {
        public override UnitType UnitType => UnitType.Magician;

        public override void ManualUpdate()
        {
            if (AttackCooldown > Model.MaxAttackCooldown - Model.PostAttackDelay)
            {
                CurrentAttackCooldown -= Time.deltaTime;
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

            IProjectile fireball = Model.FireballPool.RequestPoolableObject();
            fireball.Launch(this, target);

            Animator.SetTrigger("Attack");
            ResetAttackCooldown();
        }
    }
}