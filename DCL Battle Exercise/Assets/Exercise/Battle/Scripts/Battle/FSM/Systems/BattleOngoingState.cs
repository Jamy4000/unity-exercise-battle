using Utils;

namespace DCLBattle.Battle
{
    public class BattleOngoingState : BattleState<BattleOngoingStateData>
    {
        private readonly IArmiesHolder _armiesHolder;

        public BattleOngoingState(BattleOngoingStateData stateData, IArmiesHolder armiesHolder) : base(stateData)
        {
            for (int armyIndex = 0; armyIndex < armiesHolder.ArmiesCount; armyIndex++)
            {
                armiesHolder.GetArmy(armyIndex).ArmyDefeatedEvent += OnArmyDefeatedEvent;
            }
            _armiesHolder = armiesHolder;
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
            MessagingSystem<StartBattleEvent>.Publish(new StartBattleEvent());
        }

        public override void UpdateState()
        {
        }

        public override void EndState()
        {
        }

        private void OnArmyDefeatedEvent(Army defeatedArmy)
        {
            defeatedArmy.ArmyDefeatedEvent -= OnArmyDefeatedEvent;

            int remainingAllianceID = -1;
            for (int armyIndex = 0; armyIndex < _armiesHolder.ArmiesCount; armyIndex++)
            {
                var army = _armiesHolder.GetArmy(armyIndex);
                // if we didn't set the remaining alliance ID yet
                if (remainingAllianceID == -1)
                {
                    // we set it to the first army we encounter with units remaining
                    if (army.RemainingUnitsCount > 0)
                        remainingAllianceID = army.Model.AllianceID;
                }
                // if another army still has unit and it is from a different alliance
                else if (army.RemainingUnitsCount > 0 && remainingAllianceID != army.Model.AllianceID)
                {
                    // we are still at war
                    return;
                }
            }

            RequestToExitState?.Invoke();
        }
    }
}