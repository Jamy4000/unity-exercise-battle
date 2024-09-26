public enum UnitType
{
    Warrior = 0,
    Archer = 1,
}

public enum ArmyStrategy
{
    Basic = 0,
    Defensive = 1
}

public interface IArmyModel
{
    ArmyStrategy Strategy { get; set; }

    int GetUnitsCount(UnitType type);
    void SetUnitsCount(UnitType type, int unitsCount);

    UnitBase GetUnitPrefab(UnitType type);
}