using UnityEngine;

namespace DCLBattle.Battle
{
    public sealed class UnitWalkState : UnitState<UnitWalkStateData>
    {
        public UnitWalkState(UnitWalkStateData stateData, UnitBase unitBase) : base(stateData, unitBase)
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