using UnityEngine;

namespace DCLBattle.Battle
{
    [CreateAssetMenu(menuName = "DCLBattle/Units/FSM/Idle State Data", fileName = "IdleStateData", order = 0)]
    public class UnitIdleStateData : UnitStateData
    {
        public override UnitStateID StateID => UnitStateID.Idle;

        public override UnitState CreateStateInstance(UnitBase unit)
        {
            return new UnitIdleState(this, unit);
        }
    }

}