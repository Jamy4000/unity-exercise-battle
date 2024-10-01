namespace DCLBattle.Battle
{
    public class UnitDyingState : UnitState<UnitDyingStateData>
    {
        public UnitDyingState(UnitDyingStateData stateData, UnitBase unit) : base(stateData, unit)
        {
            unit.UnitWasHitEvent += OnUnitWasHit;
        }

        public override void OnDestroy()
        {
        }

        public override bool CanBeEntered()
        {
            return false;
        }

        public override bool CanBeExited()
        {
            return false;
        }

        public override void StartState(UnitStateID previousState)
        {
            Unit.Animator.SetTrigger(StateData.DeathAnimName);
        }

        public override void UpdateState()
        {
        }

        public override void EndState()
        {
        }

        private void OnUnitWasHit(float newHealth)
        {
            if (newHealth <= 0f)
                RequestEnterState?.Invoke(StateEnum);
        }
    }
}