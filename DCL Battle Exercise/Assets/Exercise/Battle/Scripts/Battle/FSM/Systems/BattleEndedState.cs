using Utils;

namespace DCLBattle.Battle
{
    public class BattleEndedState : BattleState<BattleEndedStateData>, IServiceConsumer
    {
        private IArmiesHolder _armiesHolder;

        public BattleEndedState(BattleEndedStateData stateData, IServiceLocator serviceLocator) : base(stateData)
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
            return false;
        }

        public override void StartState(BattleStateID previousState)
        {
            int remainingAllianceID = -1;
            for (int armyIndex = 0; armyIndex < _armiesHolder.ArmiesCount; armyIndex++)
            {
                var army = _armiesHolder.GetArmy(armyIndex);
                if (army.RemainingUnitsCount > 0)
                    remainingAllianceID = army.Model.AllianceID;
            }

            MessagingSystem<AllianceWonEvent>.Publish(new AllianceWonEvent(remainingAllianceID));
        }

        public override void UpdateState()
        {
        }

        public override void EndState()
        {
        }

        public void ConsumeLocator(IServiceLocator locator)
        {
            _armiesHolder = locator.GetService<IArmiesHolder>();
        }
    }
}