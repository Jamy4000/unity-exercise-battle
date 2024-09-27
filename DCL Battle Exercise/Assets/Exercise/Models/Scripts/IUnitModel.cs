namespace DCLBattle.LaunchMenu
{
    public interface IUnitModel
    {
        string UnitName { get; }
        UnitType UnitType { get; }
        UnityEngine.GameObject UnitViewPrefab { get; }
    }
}