public interface IArmyModel
{
    // TODO this shouldn't be one int per army type
    int Warriors { get; set; }
    int Archers { get; set; }
    ArmyStrategy Strategy { get; set; }
}

public enum ArmyStrategy
{
    Basic = 0,
    Defensive = 1
}