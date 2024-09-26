using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class UnitBase : MonoBehaviour
{
    // todo turn those as abstract getters
    public float health { get; protected set; }
    public float defense { get; protected set; }
    public float attack { get; protected set; }
    public float maxAttackCooldown { get; protected set; }
    public float postAttackDelay { get; protected set; }
    public float speed { get; protected set; } = 0.1f;
    public abstract UnitType UnitType { get; }

    [NonSerialized]
    public IArmyModel armyModel;

    protected float attackCooldown;
    private Vector3 lastPosition;

    // TODO not a fan of having a reference to this
    private Army _army;

    protected Animator Animator { get; private set; }

    public abstract void Attack(UnitBase enemy);

    protected abstract void UpdateDefensive(List<UnitBase> allies, List<UnitBase> enemies);
    protected abstract void UpdateBasic(List<UnitBase> allies, List<UnitBase> enemies);

    protected virtual void Awake()
    {
        Animator = GetComponentInChildren<Animator>();
    }

    public virtual void Move( Vector3 delta )
    {
        if (attackCooldown > maxAttackCooldown - postAttackDelay)
            return;

        transform.position += delta * speed;
    }

    // TODO Change GameObject as ITarget
    public virtual void Hit(GameObject sourceGo )
    {
        // TODO IProjectile
        float sourceAttack = sourceGo.TryGetComponent(out UnitBase source) ?
            source.attack :
            sourceGo.GetComponent<ArcherArrow>().attack;

        health -= Mathf.Max(sourceAttack - defense, 0);

        if ( health < 0 )
        {
            transform.forward = sourceGo.transform.position - transform.position;

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
        var allies = _army.GetUnits();
        var enemies = _army.GetEnemyArmy().GetUnits();

        UpdateBasicRules(allies, enemies);

        // TODO Use an interface for the strategies
        switch ( armyModel.Strategy )
        {
            case ArmyStrategy.Defensive:
                UpdateDefensive(allies, enemies);
                break;
            case ArmyStrategy.Basic:
                UpdateBasic(allies, enemies);
                break;
        }

        Animator.SetFloat("MovementSpeed", (transform.position - lastPosition).magnitude / speed);
        lastPosition = transform.position;
    }

    void UpdateBasicRules(List<UnitBase> allies, List<UnitBase> enemies)
    {
        attackCooldown -= Time.deltaTime;
        EvadeAllies(allies, enemies);
    }

    void EvadeAllies(List<UnitBase> allies, List<UnitBase> enemies)
    {
        var allUnits = allies.Union(enemies).ToList();

        // TODO that would be nice to cache
        Vector3 center = Utils.GetCenter(allUnits);

        float centerSqDist = Vector3.SqrMagnitude(gameObject.transform.position - center);

        // TODO Hard coded value
        if ( centerSqDist > (80.0f * 80.0f))
        {
            Vector3 toNearest = Vector3.Normalize(center - transform.position);
            transform.position -= toNearest * (80.0f - Mathf.Sqrt(centerSqDist));
            return;
        }

        foreach ( var obj in allUnits )
        {
            float sqDist = Vector3.SqrMagnitude(gameObject.transform.position - obj.transform.position);

            // TODO Hard coded value
            if ( sqDist < 4f )
            {
                Vector3 toNearest = Vector3.Normalize(obj.transform.position - transform.position);
                // TODO Hard Coded value
                transform.position -= toNearest * (2f - Mathf.Sqrt(sqDist));
            }
        }
    }
}