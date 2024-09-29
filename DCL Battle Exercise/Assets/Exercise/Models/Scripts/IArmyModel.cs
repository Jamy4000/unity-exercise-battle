public enum ArmyStrategy
{
    Basic = 0,
    Defensive = 1,
    Offensive = 2,
    Chaos = 3
}

public interface IArmyModel
{
    public static readonly int UnitLength = System.Enum.GetValues(typeof(UnitType)).Length;

    string ArmyName { get; set; }

    UnityEngine.Color ArmyColor { get; set; }

    ArmyStrategy Strategy { get; set; }

    int GetUnitCount(UnitType type);
    void SetUnitCount(UnitType type, int newCount);

    bool TryGetUnitModel(UnitType type, out IUnitModel unitModel);
    IUnitModel GetUnitModel(UnitType type);

    // TODO We may want to move this away, or make the model itself the factory
    DCLBattle.Battle.IUnitFactory GetUnitFactory(UnitType type);
}