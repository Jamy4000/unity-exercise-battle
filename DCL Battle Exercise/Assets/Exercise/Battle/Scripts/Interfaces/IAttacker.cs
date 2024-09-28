using UnityEngine;

namespace DCLBattle.Battle
{
    public interface IAttacker
    {
        Vector3 Position { get; }
        IArmy Army { get; }
        void Attack(IAttackReceiver target);
    }
}