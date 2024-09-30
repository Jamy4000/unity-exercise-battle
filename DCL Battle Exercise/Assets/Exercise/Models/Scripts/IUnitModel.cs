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

    float BaseHealth { get; }
    float Defense { get; }
    float AttackRange { get; }
    float AttackCooldown { get; }

    UnitStateData[] UnitStatesData { get; }
    UnitStateID DefaultState { get; }

    IStrategyUpdater CreateStrategyUpdater(ArmyStrategy armyStrategy);
    UnitBase InstantiateUnit(UnitCreationParameters unitCreationParameters);
}