using UnityEngine;

namespace DCLBattle.Battle
{
    [CreateAssetMenu(menuName = "DCLBattle/Units/FSM/Reloading State Data", fileName = "ReloadingStateData", order = 0)]
    public sealed class UnitReloadingStateData : UnitStateData
    {
        public override UnitStateID StateID => UnitStateID.Reloading;

        public override UnitState CreateStateInstance()
        {
            return new UnitReloadingState(this);
        }
    }

}