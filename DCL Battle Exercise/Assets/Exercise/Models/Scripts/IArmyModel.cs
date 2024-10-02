public enum ArmyStrategy
{
    Basic = 0,
    Defensive = 1,
    //Offensive = 2,
    //Chaos = 3
}

public interface IArmyModel
{
    public static readonly int UnitLength = System.Enum.GetValues(typeof(UnitType)).Length - 1;
    public static readonly int StrategyCount = System.Enum.GetValues(typeof(ArmyStrategy)).Length;

    string ArmyName { get; set; }

    UnityEngine.Color ArmyColor { get; set; }

    ArmyStrategy Strategy { get; set; }

    /// <summary>
    /// Can't have Mordor fight alongside Rivendell
    /// </summary>
    int AllianceID { get; set; }

    int GetUnitCount(UnitType type);
    void SetUnitCount(UnitType type, int newCount);

    bool TryGetUnitModel(UnitType type, out IUnitModel unitModel);
    IUnitModel GetUnitModel(UnitType type);
}