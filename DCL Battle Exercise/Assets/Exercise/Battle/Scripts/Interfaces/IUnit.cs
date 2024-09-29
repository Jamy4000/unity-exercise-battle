namespace DCLBattle.Battle
{
    public interface IUnit
    {
        UnitType UnitType { get; }
        IArmy Army { get; }

        UnityEngine.Vector3 Position { get; }

        void Initialize(UnitCreationParameters parameters);
    }
}