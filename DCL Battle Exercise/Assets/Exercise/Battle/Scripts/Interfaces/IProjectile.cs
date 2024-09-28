namespace DCLBattle.Battle
{
    public interface IProjectile
    {
        void Launch(IAttacker attacker, IAttackReceiver target);
    }
}