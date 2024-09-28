using UnityEngine;

namespace DCLBattle.Battle
{
    public sealed class ArcherArrow : MonoBehaviour, IProjectile
    {
        [SerializeField]
        private float _speed;

        private float _speedSq;

        [SerializeField]
        private float _attackDamage;

        private IAttackReceiver _target;
        private IAttacker _source;

        // TODO Remove this
        public Army Army;

        private void Awake()
        {
            _speedSq = _speed * _speed;
        }

        public void Launch(IAttacker attacker, IAttackReceiver target)
        {
            _source = attacker;
            _target = target;

            transform.position = attacker.Position;
            GetComponent<Renderer>().material.color = attacker.Army.ArmyColor;
        }

        // TODO have a system go other every arrow instead of each arrow having an update
        public void Update()
        {
            Vector3 position = transform.position;
            Vector3 direction = Vector3.Normalize(_target.Position - position);
            position += direction * _speed;

            transform.position = position;
            transform.forward = direction;

            float sqDist = Vector3.SqrMagnitude(_target.Position - transform.position);

            if (sqDist < _speedSq)
            {
                _target.Hit(_source, position, _attackDamage);
                // TODO pooling
                Destroy(gameObject);
                return;
            }
        }
    }
}