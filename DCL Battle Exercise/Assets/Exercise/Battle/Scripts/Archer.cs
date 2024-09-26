using System.Collections.Generic;
using UnityEngine;

public class Archer : UnitBase
{
    [SerializeField]
    private float _attackRange = 20f;
    private float _attackRangeSq;

    [SerializeField, Interface(typeof(IProjectile))]
    private Object arrowPrefab;

    private Color _color;

    public override UnitType UnitType => UnitType.Archer;

    protected override void Awake()
    {
        base.Awake();
        _attackRangeSq = _attackRange * _attackRange;

        // TODO Move this to a SO
        health = 5;
        defense = 0;
        attack = 10;
        maxAttackCooldown = 5f;
        postAttackDelay = 1f;
    }

    private void Start()
    {
        // TODO not a fan of that one
        _color = GetComponentInChildren<Renderer>().material.color;
    }

    public override void Attack(IAttackReceiver target)
    {
        if (attackCooldown > 0)
            return;

        if (Vector3.SqrMagnitude(transform.position - target.Position) > _attackRangeSq)
            return;

        attackCooldown = maxAttackCooldown;
        // TODO Pooling
        IProjectile projectile = Instantiate(arrowPrefab) as IProjectile;
        projectile.Launch(transform.position, target);

        Animator.SetTrigger("Attack");

        projectile.GetComponent<Renderer>().material.color = _color;
    }

    public void OnDeathAnimFinished()
    {
        Destroy(gameObject);
    }

    protected override void UpdateDefensive(List<UnitBase> allies, List<UnitBase> enemies)
    {
        Vector3 enemyCenter = Utils.GetCenter(enemies);
        float distToEnemyX = Mathf.Abs(enemyCenter.x - transform.position.x);

        if (distToEnemyX > attackRange)
        {
            if (enemyCenter.x < transform.position.x)
                Move(Vector3.left);

            if (enemyCenter.x > transform.position.x)
                Move(Vector3.right);
        }

        float distToNearest = Utils.GetNearestObject(this, enemies, out UnitBase nearestEnemy);

        if (nearestEnemy == null)
            return;

        if (distToNearest < attackRange)
        {
            Vector3 toNearest = (nearestEnemy.transform.position - transform.position).normalized;
            toNearest.Scale(new Vector3(1, 0, 1));

            Vector3 flank = Quaternion.Euler(0, 90, 0) * toNearest;
            Move(-(toNearest + flank).normalized);
        }
        else
        {
            Vector3 toNearest = (nearestEnemy.transform.position - transform.position).normalized;
            toNearest.Scale(new Vector3(1, 0, 1));
            Move(toNearest.normalized);
        }

        Attack(nearestEnemy);
    }

    protected override void UpdateBasic(List<UnitBase> allies, List<UnitBase> enemies)
    {
        Utils.GetNearestObject(this, enemies, out UnitBase nearestEnemy);

        if (nearestEnemy == null)
            return;

        Vector3 toNearest = (nearestEnemy.transform.position - transform.position).normalized;
        toNearest.Scale(new Vector3(1, 0, 1));
        Move(toNearest.normalized);

        Attack(nearestEnemy);
    }
}