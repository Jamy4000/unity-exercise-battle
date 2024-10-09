using System;

namespace DCLBattle.Battle
{
    public interface IArmiesHolder : IDisposable, Utils.IService
    {
        int ArmiesCount { get; }
        Army GetArmy(int armyIndex);
        UnityEngine.Vector3 BattleCenter { get; }
    }
}