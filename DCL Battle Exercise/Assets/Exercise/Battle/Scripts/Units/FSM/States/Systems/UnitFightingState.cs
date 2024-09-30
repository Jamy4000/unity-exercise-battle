using UnityEngine;

namespace DCLBattle.Battle
{
    public class UnitFightingState : UnitState<UnitFightingStateData>
    {
        public UnitFightingState(UnitFightingStateData stateData, UnitBase unit) : base(stateData, unit)
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
            Vector3 moveOffset = CalculateUnitTowardBattleOffset();
            moveOffset += Unit.StrategyUpdater.UpdateStrategy(Unit);
            Unit.Move(moveOffset * (Time.deltaTime * StateData.UnitMoveSpeed));
        }

        private Vector3 CalculateUnitTowardBattleOffset()
        {
            // TODO fetching the service every frame for every unit sounds like a bad idea
            Vector3 center = UnityServiceLocator.ServiceLocator.Global.Get<BattleInstantiator>().BattleCenter;

            // TODO Hard coded values
            if (Vector3.SqrMagnitude(Unit.Position - center) < 80.0f * 80.0f)
                return Vector3.zero;

            Vector3 toCenter = Vector3.Normalize(center - Unit.Position);
            return toCenter * (Time.deltaTime * StateData.UnitMoveSpeed);
        }

        public override void EndState()
        {
        }
    }
}