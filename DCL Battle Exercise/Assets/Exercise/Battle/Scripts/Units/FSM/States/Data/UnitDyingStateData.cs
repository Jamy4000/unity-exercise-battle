using UnityEngine;

namespace DCLBattle.Battle
{
    [CreateAssetMenu(menuName = "DCLBattle/Units/FSM/Dying State Data", fileName = "DyingStateData", order = 0)]
    public class UnitDyingStateData : UnitStateData
    {
        public override UnitStateID StateID => UnitStateID.Dying;

        public override UnitState CreateStateInstance(UnitBase unit)
        {
            return new UnitDyingState(this, unit);
        }
    }

}