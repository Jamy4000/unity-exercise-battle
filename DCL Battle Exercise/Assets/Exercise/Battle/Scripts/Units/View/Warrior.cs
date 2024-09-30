using UnityEngine;

namespace DCLBattle.Battle
{
    public sealed class Warrior : UnitBase<WarriorModelSO>
    {
        public override UnitType UnitType => UnitType.Warrior;

        public override void Attack(IAttackReceiver target)
        {
            if (AttackCooldown > 0)
                return;

            if (Vector3.SqrMagnitude(transform.position - target.Position) > Model.AttackRangeSq)
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