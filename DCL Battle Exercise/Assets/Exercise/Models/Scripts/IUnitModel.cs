using DCLBattle.Battle;

public enum UnitType
{
    UNDEFINED = -1,
    Warrior = 0,
    Archer = 1,
    Cavalry = 2,
    Magician = 3,
}

public interface IUnitModel
{
    string UnitName { get; }
    UnitType UnitType { get; }

    IStrategyUpdater CreateStrategyUpdater(ArmyStrategy armyStrategy);
    IUnit InstantiateUnit(UnitCreationParameters unitCreationParameters);
}