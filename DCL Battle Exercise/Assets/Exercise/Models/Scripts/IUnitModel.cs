using DCLBattle.Battle;

public enum UnitType
{
    Warrior = 0,
    Archer = 1,
    Cavalry = 2,
    Magician = 3,
}

public interface IUnitModel
{
    string UnitName { get; }
    UnitType UnitType { get; }
    IUnitFactory UnitFactory { get; }

    IStrategyUpdater CreateStrategyUpdater(ArmyStrategy armyStrategy);
}