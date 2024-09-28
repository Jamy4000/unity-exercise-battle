using UnityEngine;

namespace DCLBattle.Battle
{
    public interface IAttackReceiver
    {
        Vector3 Position { get; }
        void Hit(IAttacker attacker, Vector3 hitPosition, float damage);
    }
}