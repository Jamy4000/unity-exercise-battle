using System.Collections.Generic;
using UnityEngine;

public class Archer : UnitBase
{
    public float attackRange = 20f;

    public ArcherArrow arrowPrefab;

    private Color _color;

    protected override void Awake()
    {
        base.Awake();
        health = 5;
        defense = 0;
        attack = 10;
        maxAttackCooldown = 5f;
        postAttackDelay = 1f;
    }

    private void Start()
    {
        // TODO not a fan of that one
        _color = GetComponent<Renderer>().material.color;
    }

    public override void Attack(UnitBase enemy)
    {
        if ( attackCooldown > 0 )
            return;

        if ( Vector3.Distance(transform.position, enemy.transform.position) > attackRange )
            return;

        attackCooldown = maxAttackCooldown;
        // TODO IProjectile
        ArcherArrow arrow = Object.Instantiate(arrowPrefab.gameObject).GetComponent<ArcherArrow>();
        arrow.target = enemy.transform.position;
        arrow.attack = attack;
        arrow.transform.position = transform.position;

        Animator.SetTrigger("Attack");

        arrow.GetComponent<Renderer>().material.color = _color;
    }

    public void OnDeathAnimFinished()
    {
        Destroy(gameObject);
    }

    protected override void UpdateDefensive(List<UnitBase> allies, List<UnitBase> enemies)
    {
        Vector3 enemyCenter = Utils.GetCenter(enemies);
        float distToEnemyX = Mathf.Abs( enemyCenter.x - transform.position.x );

        if ( distToEnemyX > attackRange )
        {
            if ( enemyCenter.x < transform.position.x )
                Move( Vector3.left );

            if ( enemyCenter.x > transform.position.x )
                Move( Vector3.right );
        }

        float distToNearest = Utils.GetNearestObject(this, enemies, out UnitBase nearestEnemy );

        if ( nearestEnemy == null )
            return;

        if ( distToNearest < attackRange )
        {
            Vector3 toNearest = (nearestEnemy.transform.position - transform.position).normalized;
            toNearest.Scale( new Vector3(1, 0, 1));

            Vector3 flank = Quaternion.Euler(0, 90, 0) * toNearest;
            Move( -(toNearest + flank).normalized );
        }
        else
        {
            Vector3 toNearest = (nearestEnemy.transform.position - transform.position).normalized;
            toNearest.Scale( new Vector3(1, 0, 1));
            Move( toNearest.normalized );
        }

        Attack(nearestEnemy);
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