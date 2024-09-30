using UnityEngine;

namespace DCLBattle.Battle
{
    [CreateAssetMenu(menuName = "DCLBattle/Units/FSM/Attacking State Data", fileName = "AttackingStateData", order = 0)]
    public sealed class UnitAttackingStateData : UnitStateData
    {
        public override UnitStateID StateID => UnitStateID.Attacking;

        public override UnitState CreateStateInstance(UnitBase unit)
        {
            return new UnitAttackingState(this, unit);
        }
    }

}