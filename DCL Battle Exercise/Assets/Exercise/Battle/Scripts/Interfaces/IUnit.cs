namespace DCLBattle.Battle
{
    public interface IUnit
    {
        UnityEngine.Vector3 Position { get; }
        IArmy Army { get; }

        void Initialize(UnitCreationParameters parameters);
    }
}