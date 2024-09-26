public enum UnitType
{
    Warrior = 0,
    Archer = 1,
    Cavalry = 2,
    Magician = 3,
}

public enum ArmyStrategy
{
    Basic = 0,
    Defensive = 1
}

public interface IArmyModel
{
    string ArmyName { get; set; }

    UnityEngine.Color ArmyColor { get; set; }

    ArmyStrategy Strategy { get; set; }

    int GetUnitsCount(UnitType type);
    void SetUnitsCount(UnitType type, int unitsCount);

    IUnitModel GetUnitModel(UnitType type);
}