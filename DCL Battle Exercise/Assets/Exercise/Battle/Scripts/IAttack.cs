using UnityEngine;

public interface IProjectile
{
    void Setup(IAttacker attacker);
    void Launch(Vector3 startPosition, IAttackReceiver target);
}

public interface IAttackReceiver
{
    Vector3 Position { get; }
    void Hit(IAttacker attacker, Vector3 hitPosition, float damage);
}

public interface IAttacker
{
    Vector3 Position { get; }
    int ArmyID { get; }
    // TODO We should get this using the Army ID
    Color ArmyColor { get; }
    void Attack(IAttackReceiver target);
}