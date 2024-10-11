using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace DCLBattle.Battle
{
    /// <summary>
    /// A variant to UnitBase that allows us to get the specific model implementation in the derived class
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public abstract class UnitBase<TModel> : UnitBase where TModel : class, IUnitModel
    {
        protected TModel Model { get; private set; }

        public override float Defense => Model.Defense;
        public override float AttackRange => Model.AttackRange;
        protected float CurrentAttackCooldown;
        public override float AttackCooldown => CurrentAttackCooldown;

        // TODO Could add that Attack string in the unit data
        protected int AttackAnimHash { get; } = Animator.StringToHash("Attack");

        public override void Initialize(UnitCreationParameters parameters)
        {
            Model = parameters.UnitModel as TModel;
            base.Initialize(parameters);
        }

        public override void ManualUpdate()
        {
            CurrentAttackCooldown -= Time.deltaTime;
            base.ManualUpdate();
        }

        protected void ResetAttackCooldown()
        {
            CurrentAttackCooldown = Model.AttackCooldown;
        }

        protected override UnitFSM CreateFsm()
        {
            List<UnitState> states = new(Model.UnitStatesData.Length);
            UnitState defaultState = null;

            for (int i = 0; i < Model.UnitStatesData.Length; i++)
            {
                UnitState state = Model.UnitStatesData[i].CreateStateInstance(this);
                states.Add(state);
                if (state.StateEnum == Model.DefaultState)
                    defaultState = state;
            }

            return new UnitFSM(defaultState, states);
        }
    }

    /// <summary>
    /// This base class for Units is mainly useful to pass reference around without worrying about the Generic type
    /// </summary>
    public abstract class UnitBase : MonoBehaviour, IAttackReceiver, IAttacker, I_LateUpdateOnly
    {
        // not returning _model.UnitType in order to use the value in the OnValidate method of the Model SO
        public abstract UnitType UnitType { get; }
        public int UnitID { get; private set; }

        // TODO this crossdependency feels wrong, I'd rather have an ID instead
        public Army Army { get; private set; }
        public IStrategyUpdater StrategyUpdater { get; private set; }

        public Vector3 Position => _lastPosition;

        // IAttackReceiver
        public float Health => _currentHealth;
        public abstract float Defense { get; }
        public System.Action<IAttackReceiver> AttackReceiverDiedEvent { get; set; }

        // IAttacker
        public abstract float AttackRange { get; }
        public abstract float AttackCooldown { get; }

        public System.Action<float> UnitWasHitEvent { get; set; }

        public bool IsMarkedForDeletion { get; set; }
        public Animator Animator { get; private set; }
        protected UnitFSM Fsm { get; private set; }

        private readonly int _movementSpeedAnimHash = Animator.StringToHash("MovementSpeed");
        private float _currentHealth;
        private Vector3 _moveOffset;
        private Vector3 _lastPosition;

        private System.Action<UnitStateID> _cachedOnStateStartedCallback;

        protected virtual void Awake()
        {
            Animator = GetComponentInChildren<Animator>();
            _cachedOnStateStartedCallback = OnStateStarted;
        }

        // Create when instantiated
        public virtual void Initialize(UnitCreationParameters parameters)
        {
            Army = parameters.ParentArmy;
            StrategyUpdater = parameters.StrategyUpdater;
            UnitID = parameters.UnitID;

            Fsm = CreateFsm();
            Fsm.RegisterStateStartedCallback(_cachedOnStateStartedCallback);

            _currentHealth = parameters.UnitModel.BaseHealth;
            
            GetComponentInChildren<Renderer>().material.color = Army.Model.ArmyColor;
            transform.SetPositionAndRotation(parameters.Position, parameters.Rotation);
            transform.name = $"{parameters.ParentArmy.Model.ArmyName} - {parameters.UnitModel.UnitName}";

            _lastPosition = transform.position;

            GameUpdater.Register(this);
        }

        public virtual void ManualUpdate()
        {
            _moveOffset = Vector3.zero;

            Fsm.ManualUpdate();

            transform.position += _moveOffset;

            // TODO I don't think this should be here + hard coded value for Speed
            Animator.SetFloat(_movementSpeedAnimHash, (transform.position - _lastPosition).magnitude / 20f);
            _lastPosition = transform.position;
        }

        public virtual void ManualLateUpdate()
        {
            Fsm.LateUpdate();
        }

        protected virtual void OnDestroy()
        {
            Fsm.UnregisterStateStartedCallback(_cachedOnStateStartedCallback);

            GameUpdater.Unregister(this);
        }

        public virtual void Move(Vector3 delta)
        {
            _moveOffset += delta;
        }

        public virtual void Hit(IAttacker attacker, Vector3 hitPosition, float damage)
        {
            _currentHealth -= Mathf.Max(damage - Defense, 0f);
            UnitWasHitEvent?.Invoke(_currentHealth);
        }

        private void OnStateStarted(UnitStateID newState)
        {
            if (newState == UnitStateID.Dying)
            {
                AttackReceiverDiedEvent?.Invoke(this);
            }
        }

        protected abstract UnitFSM CreateFsm();
        public abstract void Attack(IAttackReceiver attackReceiver);
    }
}