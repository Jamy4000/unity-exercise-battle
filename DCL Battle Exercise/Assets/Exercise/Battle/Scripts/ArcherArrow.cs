using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherArrow : MonoBehaviour
{
    public float speed;

    [NonSerialized] public Vector3 target;
    [NonSerialized] public float attack;

    public Army army;

    // TODO have a system go other every arrow instead of each arrow having an update
    public void Update()
    {
        Vector3 position = transform.position;
        Vector3 direction = Vector3.Normalize(target - position);
        position += direction * speed;
        
        transform.position = position;
        transform.forward = direction;

        // TODO Octree or KDTree
        foreach ( var a in army.GetEnemyArmy().GetUnits() )
        {
            float dist = Vector3.Distance(a.transform.position, transform.position);

            if (dist < speed)
            {
                UnitBase unit = a.GetComponent<UnitBase>();
                unit.Hit(gameObject);
                // TODO pooling
                Destroy(gameObject);
                return;
            }
        }

        if ( Vector3.Distance(transform.position, target) < speed)
        {
            Destroy(gameObject);
        }
    }
}