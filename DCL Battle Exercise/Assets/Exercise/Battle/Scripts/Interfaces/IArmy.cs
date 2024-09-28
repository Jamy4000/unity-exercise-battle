namespace DCLBattle.Battle
{
    public interface IArmy
    {
        UnityEngine.Color ArmyColor { get; }
        ArmyStrategy Strategy { get; }
        int RemainingUnitsCount { get; }

        UnityEngine.Vector3 CalculateCenterPoint();

        void Update();

        void AddUnit(UnitBase unit);
        void RemoveUnit(UnitBase unit);
    }
}