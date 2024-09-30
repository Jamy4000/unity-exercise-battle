using static UnityEngine.UI.CanvasScaler;

namespace DCLBattle.Battle
{
    public sealed class UnitReloadingState : UnitState<UnitReloadingStateData>
    {
        public UnitReloadingState(UnitReloadingStateData stateData, UnitBase unit) : base(stateData, unit)
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