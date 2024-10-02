using System;

namespace DCLBattle.Battle
{
    public interface IArmiesHolder : IDisposable
    {
        int ArmiesCount { get; }
        Army GetArmy(int armyIndex);
        UnityEngine.Vector3 BattleCenter { get; }
    }
}