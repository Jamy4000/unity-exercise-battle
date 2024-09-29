namespace DCLBattle.Battle
{
    public interface IUnit
    {
        UnitType UnitType { get; }
        IArmy Army { get; }

        UnityEngine.Vector3 Position { get; }

        void Initialize(UnitCreationParameters parameters);
        void Move(UnityEngine.Vector3 direction);
        // TODO Breaking interface responsibility, this should be in IAttacker
        void Attack(IAttackReceiver unit);
    }
}