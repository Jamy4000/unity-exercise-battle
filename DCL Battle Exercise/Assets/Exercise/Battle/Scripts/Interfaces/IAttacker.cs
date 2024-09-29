using UnityEngine;

namespace DCLBattle.Battle
{
    public interface IAttacker
    {
        Vector3 Position { get; }
        IArmy Army { get; }

        float Damage { get; }
        float MaxAttackCooldown { get; }
        float PostAttackDelay { get; }

        void Attack(IAttackReceiver target);
    }
}