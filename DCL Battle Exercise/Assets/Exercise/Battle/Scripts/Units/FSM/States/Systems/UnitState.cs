using System.Collections.Generic;
using System;
using Utils;

namespace DCLBattle.Battle
{
    public abstract class UnitState : IFSMState<UnitStateID>
    {
        public abstract UnitStateID StateEnum { get; }

        public Action<UnitStateID> RequestEnterState { get; set; }
        public Action RequestToExitCurrentState { get; set; }
        public bool IsActiveState { get; set; }

        public abstract bool CanBeEntered();
        public virtual bool CanBeExited() => true;

        public abstract void StartState(UnitStateID previousState);
        public abstract void UpdateState();
        public abstract void EndState();

        public abstract bool HasPossibleTransitionsTo(UnitStateID stateEnum);
        public abstract List<UnitStateID> GetTransitionsStates();

        public abstract void OnDestroy();
    }

    public abstract class UnitState<TData> : UnitState
        where TData : UnitStateData
    {
        protected readonly TData StateData;
        protected readonly UnitBase Unit;

        public override UnitStateID StateEnum => StateData.StateID;

        protected UnitState(TData stateData, UnitBase unit)
        {
            StateData = stateData;
            Unit = unit;
        }

        public override List<UnitStateID> GetTransitionsStates()
        {
            return StateData.ExitStates;
        }

        public override bool HasPossibleTransitionsTo(UnitStateID stateEnum)
        {
            return StateData.ExitStates.Contains(stateEnum);
        }
    }
}