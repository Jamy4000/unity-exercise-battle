using UnityEngine;

public interface IProjectile
{
    void Launch(Vector3 startPosition, IAttackReceiver target);
}

public interface IAttackReceiver
{
    Vector3 Position { get; }
    void Hit(Vector3 hitPosition, float damage);
}