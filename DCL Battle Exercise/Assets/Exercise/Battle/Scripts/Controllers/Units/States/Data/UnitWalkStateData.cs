using UnityEngine;

namespace DCLBattle.Battle
{
    [CreateAssetMenu(menuName = "DCLBattle/Units/FSM/Walk State Data", fileName = "WalkStateData", order = 0)]
    public sealed class UnitWalkStateData : UnitStateData
    {
        public override UnitStateID StateID => UnitStateID.Walk;

        public override UnitState CreateStateInstance()
        {
            return new UnitWalkState(this);
        }
    }

}