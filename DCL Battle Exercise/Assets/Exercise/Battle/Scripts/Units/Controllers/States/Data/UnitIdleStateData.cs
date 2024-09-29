using UnityEngine;

namespace DCLBattle.Battle
{
    [CreateAssetMenu(menuName = "DCLBattle/Units/FSM/Idle State Data", fileName = "IdleStateData", order = 0)]
    public sealed class UnitIdleStateData : UnitStateData
    {
        public override UnitStateID StateID => UnitStateID.Idle;

        public override UnitState CreateStateInstance()
        {
            return new UnitIdleState(this);
        }
    }

}