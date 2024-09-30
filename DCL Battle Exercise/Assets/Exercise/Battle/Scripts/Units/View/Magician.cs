using UnityEngine;

namespace DCLBattle.Battle
{
    // THis is really a copy of Archer class so far
    public sealed class Magician : UnitBase<MagicianModelSO>
    {
        public override UnitType UnitType => UnitType.Magician;

        public override void Move(Vector3 delta)
        {
            // TODO We could avoid a bunch of calculations if we were to check this before updating the strategy and evade plan
            if (AttackCooldown > Model.AttackCooldown - Model.PostAttackDelay)
                return;

            base.Move(delta);
        }

        public override void Attack(IAttackReceiver target)
        {
            if (AttackCooldown > 0f)
                return;

            if (Vector3.SqrMagnitude(transform.position - target.Position) > Model.AttackRangeSq)
                return;

            // TODO Pooling
            IProjectile fireBall = Instantiate(Model.FireBallPrefab) as IProjectile;
            fireBall.Launch(this, target);

            Animator.SetTrigger("Attack");
            ResetAttackCooldown();
        }

        public void OnDeathAnimFinished()
        {
            Destroy(gameObject);
        }
    }
}