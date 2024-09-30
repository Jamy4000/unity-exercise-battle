using UnityEngine;

namespace DCLBattle.Battle
{
    public interface IAttackReceiver
    {
        Vector3 Position { get; }

        float Health { get; }
        float Defense { get; }

        void Hit(IAttacker attacker, Vector3 hitPosition, float damage);

        void RegisterOnDeathCallback(System.Action callback);
        void UnregisterOnDeathCallback(System.Action callback);
    }
}