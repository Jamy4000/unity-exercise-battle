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
    int[] UnitsCount { get; }
    UnitBase GetUnitPrefab(UnitType type);
}