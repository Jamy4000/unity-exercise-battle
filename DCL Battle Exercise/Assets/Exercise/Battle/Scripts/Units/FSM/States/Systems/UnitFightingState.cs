using UnityEngine;

namespace DCLBattle.Battle
{
    public class UnitFightingState : UnitState<UnitFightingStateData>
    {
        private readonly (UnitBase unit, float distance)[] _unitsInRadius = new (UnitBase, float)[16];

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

        public override bool CanBeExited()
        {
            return Unit.Health <= 0f;
        }

        public override void StartState(UnitStateID previousState)
        {
        }

        public override void UpdateState()
        {
            // We first make sure the unit is staying around the battle
            Vector3 moveOffset = CalculateEvadeAlliesOffset();

            // We then calculate the move offset for this unit using the strategy of the army
            moveOffset += Unit.StrategyUpdater.UpdateStrategy(Unit);

            moveOffset = Vector3.Normalize(moveOffset);

            // We finally move the unit
            Unit.Move(moveOffset * StateData.UnitMoveSpeed);
        }

        private Vector3 CalculateEvadeAlliesOffset()
        {
            // First, we check that the unit isn't too far from the center of the battle
            Vector3 center = Unit.Army.ArmiesHolder.BattleCenter;
            Vector3 unitToCenter = center - Unit.Position;
            float centerDistanceSq = Vector3.SqrMagnitude(unitToCenter);

            // If unit is too far from the battle's center point
            if (centerDistanceSq > StateData.MaxDistanceFromCenterSq)
            {
                // we move them to the center, regardless of them overlapping another unit or not
                float centerDistance = Mathf.Sqrt(centerDistanceSq);

                // normalizing
                unitToCenter /= centerDistance;

                return unitToCenter * (centerDistance - StateData.MaxDistanceFromCenter);
            }

            // if the unit is close enough from the battle, we make sure they are not overlapping other units
            var armiesHolder = Unit.Army.ArmiesHolder;

            Vector3 moveOffset = Vector3.zero;
            // for every armies on the map
            for (int armyIndex = 0; armyIndex < armiesHolder.ArmiesCount; armyIndex++)
            {
                var army = armiesHolder.GetArmy(armyIndex);

                // We check to find the units within X radius of the current unit
                int unitsInRadiusCount = army.GetUnitsInRadius_NoAlloc(Unit.Position, StateData.MinDistanceFromOtherUnits, _unitsInRadius);

                // for every unit within that radius
                for (int unitIndex = 0; unitIndex < unitsInRadiusCount; unitIndex++)
                {
                    // we move our unit away
                    UnitBase otherUnit = _unitsInRadius[unitIndex].unit;
                    float distance = _unitsInRadius[unitIndex].distance;

                    Vector3 toNearest = Vector3.Normalize(otherUnit.Position - Unit.Position);
                    toNearest *= (StateData.MinDistanceFromOtherUnits - distance);
                    moveOffset -= toNearest;
                }
            }

            return moveOffset;
        }

        public override void EndState()
        {
        }
    }
}