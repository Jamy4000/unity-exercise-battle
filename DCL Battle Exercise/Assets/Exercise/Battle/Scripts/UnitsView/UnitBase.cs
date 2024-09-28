using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCLBattle.Battle
{
    public abstract class UnitBase : MonoBehaviour, IAttackReceiver
    {
        // todo turn those as abstract getters
        public float health { get; protected set; }
        public float defense { get; protected set; }
        public float attack { get; protected set; }
        public float maxAttackCooldown { get; protected set; }
        public float postAttackDelay { get; protected set; }
        public float speed { get; protected set; } = 0.1f;
        
        public abstract UnitType UnitType { get; }

        // TODO Inject this on Spawn
        public IArmy Army { get; set; }

        protected float _attackCooldown;
        private Vector3 _lastPosition;

        // TODO seperate this with a View component
        protected Animator Animator { get; private set; }

        public Vector3 Position => transform.position;

        protected abstract void UpdateDefensive(List<UnitBase> allies, List<UnitBase> enemies);
        protected abstract void UpdateBasic(List<UnitBase> allies, List<UnitBase> enemies);

        protected virtual void Awake()
        {
            Animator = GetComponentInChildren<Animator>();
        }

        protected virtual void Start()
        {
            // TODO do this when pooling creates the unit
            GetComponentInChildren<Renderer>().material.color = Army.ArmyColor;
        }

        public virtual void Move(Vector3 delta)
        {
            if (_attackCooldown > maxAttackCooldown - postAttackDelay)
                return;

            transform.position += delta * speed;
        }

        public virtual void Hit(IAttacker attacker, Vector3 hitPosition, float damage)
        {
            health -= Mathf.Max(damage - defense, 0);

            if (health < 0)
            {
                transform.forward = attacker.Position - transform.position;

                // TODO event for this
                //army.RemoveUnit(this);

                Animator.SetTrigger("Death");
                this.enabled = false;
            }
            else
            {
                Animator.SetTrigger("Hit");
            }
        }

        public virtual void ManualUpdate()
        {
            /*
            var allies = Army.GetUnits();
            var enemies = Army.GetEnemyArmy().GetUnits();

            UpdateBasicRules(allies, enemies);

            // TODO Use an interface for the strategies
            switch (Army.Strategy)
            {
                case ArmyStrategy.Defensive:
                    UpdateDefensive(allies, enemies);
                    break;
                case ArmyStrategy.Basic:
                    UpdateBasic(allies, enemies);
                    break;
            }

            Animator.SetFloat("MovementSpeed", (transform.position - _lastPosition).magnitude / speed);
            _lastPosition = transform.position;
            */
        }

        void UpdateBasicRules(List<UnitBase> allies, List<UnitBase> enemies)
        {
            _attackCooldown -= Time.deltaTime;
            EvadeAllies(allies, enemies);
        }

        void EvadeAllies(List<UnitBase> allies, List<UnitBase> enemies)
        {
            var allUnits = allies.Union(enemies).ToList();

            // TODO that would be nice to cache
            Vector3 center = Utils.GetCenter(allUnits);

            float centerSqDist = Vector3.SqrMagnitude(gameObject.transform.position - center);

            // TODO Hard coded value
            if (centerSqDist > (80.0f * 80.0f))
            {
                Vector3 toNearest = Vector3.Normalize(center - transform.position);
                transform.position -= toNearest * (80.0f - Mathf.Sqrt(centerSqDist));
                return;
            }

            foreach (var obj in allUnits)
            {
                float sqDist = Vector3.SqrMagnitude(gameObject.transform.position - obj.transform.position);

                // TODO Hard coded value
                if (sqDist < 4f)
                {
                    Vector3 toNearest = Vector3.Normalize(obj.transform.position - transform.position);
                    // TODO Hard Coded value
                    transform.position -= toNearest * (2f - Mathf.Sqrt(sqDist));
                }
            }
        }
    }
}