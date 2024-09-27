using DCLBattle.LaunchMenu;

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
    public static readonly int UnitLength = System.Enum.GetValues(typeof(UnitType)).Length;
        
    string ArmyName { get; set; }

    UnityEngine.Color ArmyColor { get; set; }

    ArmyStrategy Strategy { get; set; }

    int GetUnitCount(UnitType type);
    void SetUnitCount(UnitType type, int newCount);

    IUnitModel GetUnitModel(UnitType type);
}