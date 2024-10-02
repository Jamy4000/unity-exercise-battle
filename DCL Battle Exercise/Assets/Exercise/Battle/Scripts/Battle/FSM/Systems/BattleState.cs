using System.Collections.Generic;
using System;
using Utils;

namespace DCLBattle.Battle
{
    public abstract class BattleState : IFSMState<BattleStateID>
    {
        public abstract BattleStateID StateEnum { get; }

        public Action<BattleStateID> RequestEnterState { get; set; }
        public Action RequestToExitCurrentState { get; set; }
        public bool IsActiveState { get; set; }

        public abstract bool CanBeEntered();
        public abstract bool CanBeExited();

        public abstract void StartState(BattleStateID previousState);
        public abstract void UpdateState();
        public abstract void EndState();

        public abstract bool HasPossibleTransitionsTo(BattleStateID stateEnum);
        public abstract List<BattleStateID> GetTransitionsStates();

        public abstract void OnDestroy();
    }

    public abstract class BattleState<TData> : BattleState
        where TData : BattleStateData
    {
        protected readonly TData StateData;

        public override BattleStateID StateEnum => StateData.StateID;

        protected BattleState(TData stateData)
        {
            StateData = stateData;
        }

        public override List<BattleStateID> GetTransitionsStates()
        {
            return StateData.ExitStates;
        }

        public override bool HasPossibleTransitionsTo(BattleStateID stateEnum)
        {
            return StateData.ExitStates.Contains(stateEnum);
        }
    }
}