namespace DCLBattle.Battle
{
    public sealed class UnitAttackingState : UnitState<UnitAttackingStateData>
    {
        public UnitAttackingState(UnitAttackingStateData stateData) : base(stateData)
        {
        }

        public override void OnDestroy()
        {
        }

        public override bool CanBeEntered()
        {
            return true;
        }

        public override void StartState(UnitStateID previousState)
        {
        }

        public override void EndState()
        {
        }
    }
}