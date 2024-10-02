using System;
using UnityEngine;
using Utils;

namespace DCLBattle.Battle
{
    public sealed class ArcherArrow : MonoBehaviour, IProjectile, I_UpdateOnly
    {
        [SerializeField]
        private float _speed = 10f;

        [SerializeField]
        private float _distanceBeforeDespawn = 1f;
        private float _distanceSqBeforeDespawn;

        // TODO Maybe this should be controlled by the archer ?
        [SerializeField]
        private float _attackDamage = 5f;

        private IAttackReceiver _target;
        private IAttacker _source;

        public Action<IGenericPoolable> OnShouldReturnToPool { get; set; }

        private void Awake()
        {
            _distanceSqBeforeDespawn = _distanceBeforeDespawn * _distanceBeforeDespawn;
        }

        public void Launch(IAttacker attacker, IAttackReceiver target)
        {
            _source = attacker;
            _target = target;
            _target.AttackReceiverDiedEvent += OnTargetDied;

            transform.position = attacker.Position;

            GetComponent<Renderer>().material.color = attacker.Army.Model.ArmyColor;
        }

        public void ManualUpdate()
        {
            // I hate this, but since the gameUpdater needs to wait until end of frame to actually unregister 
            // there may be one frame in which we try to access a dead target
            if (_target == null)
                return;

            Vector3 position = transform.position;
            Vector3 toTarget = _target.Position - position;
            float sqDist = Vector3.SqrMagnitude(toTarget);

            Vector3 direction = toTarget / Mathf.Sqrt(sqDist);
            position += direction * (_speed * Time.deltaTime);

            transform.position = position;
            transform.forward = direction;

            if (sqDist < _distanceSqBeforeDespawn)
            {
                _target.Hit(_source, position, _attackDamage);
                OnShouldReturnToPool?.Invoke(this);
                return;
            }
        }

        private void OnTargetDied(IAttackReceiver attackReceiver)
        {
            // This usually happens when the target dies before the arrow reaches it
            OnShouldReturnToPool?.Invoke(this);
            GameUpdater.Unregister(this);
        }

        // IGenericPoolable
        // Raised when pooled and enabled
        public void Enable()
        {
            GameUpdater.Register(this);
            gameObject.SetActive(true);
        }

        // Raised when returned to pool and disabled
        public void Disable()
        {
            _target.AttackReceiverDiedEvent -= OnTargetDied;
            gameObject.SetActive(false);
            GameUpdater.Unregister(this);
        }

        // Called when object is destroy out of pool
        public void Destroy()
        {
            GameUpdater.Unregister(this);
            GameObject.Destroy(gameObject);
        }
    }
}