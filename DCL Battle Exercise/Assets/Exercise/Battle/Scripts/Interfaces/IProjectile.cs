using Utils;

namespace DCLBattle.Battle
{
    public interface IProjectile : IGenericPoolable
    {
        void Launch(IAttacker attacker, IAttackReceiver target);
    }
}