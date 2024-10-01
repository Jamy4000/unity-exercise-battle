using UnityEngine;

namespace DCLBattle.Battle
{
    [CreateAssetMenu(menuName = "DCLBattle/Units/FSM/Dying State Data", fileName = "DyingStateData", order = 0)]
    public class UnitDyingStateData : UnitStateData
    {
        public override UnitStateID StateID => UnitStateID.Dying;

        [SerializeField]
        private string _deathAnimName = "Death";
        public string DeathAnimName => _deathAnimName;

        public override UnitState CreateStateInstance(UnitBase unit)
        {
            return new UnitDyingState(this, unit);
        }
    }

}