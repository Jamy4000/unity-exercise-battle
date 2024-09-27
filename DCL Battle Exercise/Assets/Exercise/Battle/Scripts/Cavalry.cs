using System;
using System.Collections.Generic;
using UnityEngine;

// copy of warrior, just for the sake of argument
public class Cavalry : UnitBase, IAttacker
{
    private float _attackRange = 2.5f;

    private float _attackRangeSq;

    public int ArmyID  => throw new System.NotImplementedException();
    public Color ArmyColor  => throw new System.NotImplementedException();
    
    public override UnitType UnitType => UnitType.Cavalry;

    protected override void Awake()
    {
        base.Awake();
        _attackRangeSq = _attackRange * _attackRange;
        
        // TODO Move this in SO
        health = 50;
        defense = 5;
        attack = 20;
        maxAttackCooldown = 1f;
        postAttackDelay = 0;
    }

    public void Attack(IAttackReceiver target )
    {
        if ( attackCooldown > 0 )
            return;

        if ( Vector3.SqrMagnitude(transform.position - target.Position) > _attackRangeSq )
            return;

        attackCooldown = maxAttackCooldown;

        Animator.SetTrigger("Attack");

        target.Hit(this, target.Position, attack );
    }

    public void OnDeathAnimFinished()
    {
        // TODO Pooling
        Destroy(gameObject);
    }


    protected override void UpdateDefensive(List<UnitBase> allies, List<UnitBase> enemies)
    {
        Vector3 enemyCenter = Utils.GetCenter(enemies);

        // TODO Hard coded value
        if ( Mathf.Abs( enemyCenter.x - transform.position.x ) > 20f )
        {
            if ( enemyCenter.x < transform.position.x )
                Move( Vector3.left );

            if ( enemyCenter.x > transform.position.x )
                Move( Vector3.right );
        }

        Utils.GetNearestObject(this, enemies, out UnitBase nearestObject);

        if ( nearestObject == null )
            return;

        if (attackCooldown <= 0)
        {
            Move( (nearestObject.transform.position - transform.position).normalized );
        }
        else
        {
            Move( (nearestObject.transform.position - transform.position).normalized * -1 );
        }

        Attack(nearestObject);
    }

    protected override void UpdateBasic(List<UnitBase> allies, List<UnitBase> enemies)
    {
        Utils.GetNearestObject(this, enemies, out UnitBase nearestEnemy );

        if ( nearestEnemy == null )
            return;

        Vector3 toNearest = (nearestEnemy.transform.position - transform.position).normalized;
        toNearest.Scale( new Vector3(1, 0, 1));
        Move( toNearest.normalized );

        Attack(nearestEnemy);
    }
}