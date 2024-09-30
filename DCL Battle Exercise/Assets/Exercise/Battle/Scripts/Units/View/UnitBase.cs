using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCLBattle.Battle
{
    public abstract class UnitBase : MonoBehaviour, IAttackReceiver
    {
        [Header("FSM Data"), SerializeField]
        private UnitStateData[] _unitStatesData;

        [SerializeField]
        private UnitStateID _defaultState = UnitStateID.Idle;

        protected UnitFSM Fsm { get; private set; } = null;
        public Army Army { get; private set; }
        public Vector3 Position => transform.position;

        public abstract UnitType UnitType { get; }

        protected Animator Animator { get; private set; }

        public IStrategyUpdater StrategyUpdater { get; private set; }

        // TODO static for now as I don't see why we would want to have that for every unit, except if we end up threading this
        private static readonly (UnitBase unit, float distance)[] _unitsInRadius = new (UnitBase, float)[16];

        // TODO
        public float Health { get; private set; } = 10f;
        public float Defense => 2f;
        public float AttackRange => 20f; // TODO

        private Vector3 _moveOffset;
        private Vector3 _lastPosition;

        protected virtual void Awake()
        {
            Animator = GetComponentInChildren<Animator>();
            InitializeFsm();
        }

        // Create when instantiated
        public virtual void Initialize(UnitCreationParameters parameters)
        {
            Army = parameters.ParentArmy;
            StrategyUpdater = parameters.StrategyUpdater;

            GetComponentInChildren<Renderer>().material.color = Army.Model.ArmyColor;
            transform.SetPositionAndRotation(parameters.Position, parameters.Rotation);
            transform.name = $"{parameters.ParentArmy.Model.ArmyName} - {parameters.UnitType}";

            _lastPosition = transform.position;
        }

        private void InitializeFsm()
        {
            List<UnitState> states = new(_unitStatesData.Length);
            UnitState defaultState = null;

            for (int i = 0; i < _unitStatesData.Length; i++)
            {
                UnitState state = _unitStatesData[i].CreateStateInstance(this);
                states.Add(state);
                if (state.StateEnum == _defaultState)
                    defaultState = state;
            }

            Fsm = new UnitFSM(defaultState, states);
        }

        protected virtual void Update()
        {
            EvadeCloseUnits();

            Fsm.ManualUpdate();

            transform.position += _moveOffset;
            _moveOffset = Vector3.zero;

            // TODO I don't think this should be here + hard coded value for Speed
            Animator.SetFloat("MovementSpeed", (transform.position - _lastPosition).magnitude / 20f);
            _lastPosition = transform.position;
        }

        private void LateUpdate()
        {
            Fsm.ManualLateUpdate();
        }

        public virtual void Move(Vector3 delta)
        {
            _moveOffset += delta;
        }

        public virtual void Hit(IAttacker attacker, Vector3 hitPosition, float damage)
        {
            Health -= Mathf.Max(damage - Defense, 0);

            if (Health < 0)
            {
                transform.forward = attacker.Position - transform.position;

                // TODO Death Event needs to be fired
                Army.RemoveUnit(this);

                Animator.SetTrigger("Death");
                this.enabled = false;
            }
            else
            {
                Animator.SetTrigger("Hit");
            }
        }

        private void EvadeCloseUnits()
        {
            // TODO we should not be doing that every frame for every unit
            var battleInstantiator = UnityServiceLocator.ServiceLocator.Global.Get<BattleInstantiator>();

            Vector3 moveOffset = Vector3.zero;
            for (int armyIndex = 0; armyIndex < battleInstantiator.ArmiesCount; armyIndex++)
            {
                var army = battleInstantiator.GetArmy(armyIndex);
                // TODO Hard Coded value
                int unitsInRadiusCount = army.GetUnitsInRadius_NoAlloc(Position, 2f, _unitsInRadius);

                for (int unitIndex = 0; unitIndex < unitsInRadiusCount; unitIndex++)
                {
                    Vector3 toNearest = Vector3.Normalize(_unitsInRadius[unitIndex].unit.Position - transform.position);
                    // TODO Hard Coded value
                    moveOffset -= toNearest * ((2f - Mathf.Sqrt(_unitsInRadius[unitIndex].distance)) * Time.deltaTime);
                }
            }

            Move(moveOffset);
        }

        // TODO This shouldn't be here
        public abstract void Attack(IAttackReceiver attackReceiver);
    }
}