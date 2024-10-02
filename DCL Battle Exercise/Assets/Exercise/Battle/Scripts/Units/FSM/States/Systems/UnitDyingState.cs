using Utils;

namespace DCLBattle.Battle
{
    public class UnitDyingState : UnitState<UnitDyingStateData>, I_UpdateOnly
    {
        private float _deathAnimTimer;

        public UnitDyingState(UnitDyingStateData stateData, UnitBase unit) : base(stateData, unit)
        {
            unit.UnitWasHitEvent += OnUnitWasHit;
        }

        public override void OnDestroy()
        {
            Unit.UnitWasHitEvent -= OnUnitWasHit;
            UnityEngine.Object.Destroy(Unit.gameObject);
            GameUpdater.Unregister(this);
        }

        public override bool CanBeEntered()
        {
            return false;
        }

        public override bool CanBeExited()
        {
            return false;
        }

        public override void StartState(UnitStateID previousState)
        {
            Unit.Animator.SetTrigger(StateData.DeathAnimName);
            _deathAnimTimer = Unit.Animator.GetCurrentAnimatorClipInfo(0).Length;
        }

        public override void UpdateState()
        {
        }

        public void ManualUpdate()
        {
            _deathAnimTimer -= UnityEngine.Time.deltaTime;
            if (_deathAnimTimer <= 0f)
            {
                UnityEngine.Object.Destroy(Unit.gameObject);
                GameUpdater.Unregister(this);
            }
        }

        public override void EndState()
        {
        }

        private void OnUnitWasHit(float newHealth)
        {
            if (newHealth <= 0f)
            {
                Unit.UnitWasHitEvent -= OnUnitWasHit;
                RequestEnterState?.Invoke(StateEnum);
                Unit.IsMarkedForDeletion = true;
                GameUpdater.Register(this);
            }
        }
    }
}