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

        protected float AttackCooldown;

        public override void Initialize(UnitCreationParameters parameters)
        {
            Model = parameters.UnitModel as TModel;
            base.Initialize(parameters);
        }

        // TODO GameUpdater
        public override void ManualUpdate()
        {
            AttackCooldown -= Time.deltaTime;
            base.ManualUpdate();
        }

        protected void ResetAttackCooldown()
        {
            AttackCooldown = Model.AttackCooldown;
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
    public abstract class UnitBase : MonoBehaviour, IAttackReceiver, IAttacker, 
        I_UpdateOnly, I_LateUpdateOnly, ISubscriber<StartBattleEvent>, ISubscriber<AllianceWonEvent>
    {
        // not returning _model.UnitType in order to use the value in the OnValidate method of the Model SO
        public abstract UnitType UnitType { get; }

        // TODO this crossdependency feels wrong
        public Army Army { get; private set; }
        public IStrategyUpdater StrategyUpdater { get; private set; }

        public Vector3 Position => transform.position;

        // IAttackReceiver
        public float Health => _currentHealth;
        public abstract float Defense { get; }
        public System.Action<IAttackReceiver> AttackReceiverDiedEvent { get; set; }

        // IAttacker
        public abstract float AttackRange { get; }

        public System.Action<float> UnitWasHitEvent { get; set; }

        public Animator Animator { get; private set; }
        protected UnitFSM Fsm { get; private set; }

        private float _currentHealth;
        private Vector3 _moveOffset;
        private Vector3 _lastPosition;

        protected virtual void Awake()
        {
            Animator = GetComponentInChildren<Animator>();
            MessagingSystem<StartBattleEvent>.Subscribe(this);
        }

        // Create when instantiated
        public virtual void Initialize(UnitCreationParameters parameters)
        {
            Army = parameters.ParentArmy;
            StrategyUpdater = parameters.StrategyUpdater;
            MessagingSystem<AllianceWonEvent>.Subscribe(this);

            Fsm = CreateFsm();
            Fsm.RegisterStateStartedCallback(OnStateStarted);

            _currentHealth = parameters.UnitModel.BaseHealth;
            
            GetComponentInChildren<Renderer>().material.color = Army.Model.ArmyColor;
            transform.SetPositionAndRotation(parameters.Position, parameters.Rotation);
            transform.name = $"{parameters.ParentArmy.Model.ArmyName} - {parameters.UnitModel.UnitName}";

            _lastPosition = transform.position;
        }

        public virtual void ManualUpdate()
        {
            Fsm.ManualUpdate();

            transform.position += _moveOffset;
            _moveOffset = Vector3.zero;

            // TODO I don't think this should be here + hard coded value for Speed
            Animator.SetFloat("MovementSpeed", (transform.position - _lastPosition).magnitude / 20f);
            _lastPosition = transform.position;
        }

        public virtual void ManualLateUpdate()
        {
            Fsm.ManualLateUpdate();
        }

        protected virtual void OnDestroy()
        {
            Fsm.UnregisterStateStartedCallback(OnStateStarted);
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

        public void OnEvent(AllianceWonEvent evt)
        {
            GameUpdater.Unregister(this);
        }

        public void OnEvent(StartBattleEvent evt)
        {
            GameUpdater.Register(this);
        }

        private void OnStateStarted(UnitStateID newState)
        {
            if (newState == UnitStateID.Dying)
            {
                AttackReceiverDiedEvent?.Invoke(this);
                GameUpdater.Unregister(this);
            }
        }

        protected abstract UnitFSM CreateFsm();
        public abstract void Attack(IAttackReceiver attackReceiver);
    }
}