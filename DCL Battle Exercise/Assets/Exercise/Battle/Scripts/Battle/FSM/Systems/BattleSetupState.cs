using Utils;

namespace DCLBattle.Battle
{
    public class BattleSetupState : BattleState<BattleSetupStateData>, IServiceConsumer
    {
        public BattleSetupState(BattleSetupStateData stateData, IServiceLocator serviceLocator) : base(stateData)
        {
            serviceLocator.AddConsumer(this);
        }

        public override void OnDestroy()
        {
        }

        public override bool CanBeEntered()
        {
            return true;
        }

        public override bool CanBeExited()
        {
            return true;
        }

        public override void StartState(BattleStateID previousState)
        {
        }

        public override void UpdateState()
        {
        }

        public override void EndState()
        {
        }

        public void ConsumeLocator(IServiceLocator locator)
        {
            if (locator.TryGetService<IArmiesHolder>(out _))
                RequestToExitCurrentState?.Invoke();
        }
    }
}