using System.Collections.Generic;

namespace Utils
{
    public interface IFSMState
    {
        bool IsActiveState { get; set; }
        System.Action RequestToExitState { get; set; }

        bool CanBeEntered();
        bool CanBeExited();

        void OnDestroy();
    }

    public interface IFSMState<TStateEnum> : IFSMState
        where TStateEnum : struct, System.Enum
    {
        TStateEnum StateEnum { get; }

        System.Action<TStateEnum> RequestEnterState { get; set; }

        bool HasPossibleTransitionsTo(TStateEnum stateEnum);

        void StartState(TStateEnum previousState);

        void EndState();

        List<TStateEnum> GetTransitionsStates();
    }
}