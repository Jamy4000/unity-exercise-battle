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

        [SerializeField]
        private float _maxDistanceFromCenter = 80f;
        public float MaxDistanceFromCenter => _maxDistanceFromCenter;
        public float MaxDistanceFromCenterSq {get; private set; }

        [SerializeField]
        private float _minDistanceFromOtherUnits = 2f;
        public float MinDistanceFromOtherUnits => _minDistanceFromOtherUnits;

        public override UnitState CreateStateInstance(UnitBase unit)
        {
            return new UnitFightingState(this, unit);
        }

        private void OnEnable()
        {
            MaxDistanceFromCenterSq = MaxDistanceFromCenter * MaxDistanceFromCenter;
        }
    }

}