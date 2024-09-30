namespace DCLBattle.Battle
{
    public class UnitIdleState : UnitState<UnitIdleStateData>
    {
        public UnitIdleState(UnitIdleStateData stateData, UnitBase unit) : base(stateData, unit)
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