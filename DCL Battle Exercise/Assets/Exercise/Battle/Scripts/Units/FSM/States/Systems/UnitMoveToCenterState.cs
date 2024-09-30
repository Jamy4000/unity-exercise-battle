using UnityEngine;

namespace DCLBattle.Battle
{
    public sealed class UnitMoveToCenterState : UnitState<UnitMoveToCenterStateData>
    {
        public UnitMoveToCenterState(UnitMoveToCenterStateData stateData, UnitBase unitBase) : base(stateData, unitBase)
        {
        }

        public override void OnDestroy()
        {
        }

        public override bool CanBeEntered()
        {
            // TODO fetching the service every frame for every unit sounds like a bad idea
            Vector3 center = UnityServiceLocator.ServiceLocator.Global.Get<BattleInstantiator>().BattleCenter;

            float centerSqDist = Vector3.SqrMagnitude(Unit.Position - center);

            // TODO Hard coded value
            return centerSqDist > 80.0f * 80.0f;
        }

        public override void StartState(UnitStateID previousState)
        {
        }

        public override void UpdateState()
        {
            Vector3 center = UnityServiceLocator.ServiceLocator.Global.Get<BattleInstantiator>().BattleCenter;
            Vector3 toCenter = Vector3.Normalize(center - Unit.Position);
            Unit.Move(toCenter * (Time.deltaTime * StateData.Speed));

            if (Vector3.SqrMagnitude(Unit.Position - center) < 80.0f * 80.0f)
                RequestToExitState?.Invoke();
        }

        public override void EndState()
        {
        }
    }
}