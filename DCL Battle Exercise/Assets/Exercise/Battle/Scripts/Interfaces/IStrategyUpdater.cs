namespace DCLBattle.Battle
{
    public interface IStrategyUpdater
    {
        public static readonly int StrategyCount = System.Enum.GetValues(typeof(ArmyStrategy)).Length;

        public static readonly UnityEngine.Vector3 FlatScale = new UnityEngine.Vector3(1f, 0f, 1f);

        ArmyStrategy ArmyStrategy { get; }

        void UpdateStrategy(UnitBase unitToUpdate);
    }
}