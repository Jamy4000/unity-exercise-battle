using UnityEngine;

namespace DCLBattle.Battle
{
    [CreateAssetMenu(menuName = "DCLBattle/Units/FSM/Fighting State Data", fileName = "FightingStateData", order = 0)]
    public class UnitFightingStateData : UnitStateData
    {
        public override UnitStateID StateID => UnitStateID.Fighting;

        [SerializeField]
        private float _speed = 15f;
        public float UnitMoveSpeed => _speed;

        public override UnitState CreateStateInstance(UnitBase unit)
        {
            return new UnitFightingState(this, unit);
        }
    }

}