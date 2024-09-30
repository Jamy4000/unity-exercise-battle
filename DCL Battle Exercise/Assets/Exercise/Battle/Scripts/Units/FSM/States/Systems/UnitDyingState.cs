namespace DCLBattle.Battle
{
    public class UnitDyingState : UnitState<UnitDyingStateData>
    {
        public UnitDyingState(UnitDyingStateData stateData, UnitBase unit) : base(stateData, unit)
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

        public override void UpdateState()
        {
        }

        public override void EndState()
        {
        }
    }
}