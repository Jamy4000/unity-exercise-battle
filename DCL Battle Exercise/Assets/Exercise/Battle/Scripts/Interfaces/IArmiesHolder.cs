namespace DCLBattle.Battle
{
    public interface IArmiesHolder
    {
        int ArmiesCount { get; }
        Army GetArmy(int armyIndex);
        UnityEngine.Vector3 BattleCenter { get; }
    }
}