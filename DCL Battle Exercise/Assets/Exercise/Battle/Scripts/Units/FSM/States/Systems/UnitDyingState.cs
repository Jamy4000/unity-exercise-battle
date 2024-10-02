using UnityEngine;
using Utils;

namespace DCLBattle.Battle
{
    public class UnitDyingState : UnitState<UnitDyingStateData>, I_UpdateOnly
    {
        private float _deathAnimTimer;

        public UnitDyingState(UnitDyingStateData stateData, UnitBase unit) : base(stateData, unit)
        {
        }

        public override void OnDestroy()
        {
            UnityEngine.Object.Destroy(Unit.gameObject);
            GameUpdater.Unregister(this);
        }

        public override bool CanBeEntered()
        {
            return Unit.Health <= Mathf.Epsilon;
        }

        public override bool CanBeExited()
        {
            return false;
        }

        public override void StartState(UnitStateID previousState)
        {
            Unit.Animator.SetTrigger(StateData.DeathAnimName);
            _deathAnimTimer = Unit.Animator.GetCurrentAnimatorClipInfo(0).Length;
            GameUpdater.Register(this);
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
    }
}