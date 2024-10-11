using System.Collections.Generic;

using Utils;

namespace DCLBattle.Battle
{
    /// <summary>
    /// The different states in which the battle can be.
    /// /!\/!\/!\ WARNING: DO NOT update the index, as it will break Unity's Serialization.
    /// </summary>
    public enum BattleStateID
    {
        OnGoing = 0,
        Paused = 1,
        Ended = 2,
        Setup = 3
    }

    public sealed class BattleFSM : FSM<BattleState, BattleStateID>, I_LateUpdateOnly
    {
        public BattleFSM(BattleState defaultState, List<BattleState> states) :
            base(defaultState, states)
        {
            if (TryGetState(BattleStateID.Setup, out var state))
            {
                state.RequestToExitCurrentState += OnSetupDone;
            }
        }

        public void ManualLateUpdate()
        {
            LateUpdate();
            GameUpdater.Unregister(this);
        }

        private void OnSetupDone()
        {
            GameUpdater.Register(this);
            if (TryGetState(BattleStateID.Setup, out var state))
            {
                state.RequestToExitCurrentState -= OnSetupDone;
            }
        }
    }
}