﻿using UnityEngine;

namespace DCLBattle.Battle
{
    // copy of warrior, just for the sake of argument
    public sealed class Cavalry : UnitBase<CavalryModelSO>
    {
        public override UnitType UnitType => UnitType.Cavalry;

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