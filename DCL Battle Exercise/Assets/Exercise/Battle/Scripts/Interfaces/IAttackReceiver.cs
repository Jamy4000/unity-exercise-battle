using UnityEngine;

namespace DCLBattle.Battle
{
    public interface IAttackReceiver
    {
        Vector3 Position { get; }

        float Health { get; }
        float Defense { get; }

        System.Action<IAttackReceiver> AttackReceiverDiedEvent { get; set; }

        void Hit(IAttacker attacker, Vector3 hitPosition, float damage);
    }
}