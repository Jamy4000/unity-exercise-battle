using UnityEngine;

namespace DCLBattle.Battle
{
    [CreateAssetMenu(menuName = "DCLBattle/Units/FSM/Move To Center State Data", fileName = "MoveToCenterStateData", order = 0)]
    public sealed class UnitMoveToCenterStateData : UnitStateData
    {
        public override UnitStateID StateID => UnitStateID.MoveToCenter;

        [SerializeField]
        private float _speed = 2f;
        public float Speed => _speed;

        public override UnitState CreateStateInstance(UnitBase unit)
        {
            return new UnitMoveToCenterState(this, unit);
        }
    }

}