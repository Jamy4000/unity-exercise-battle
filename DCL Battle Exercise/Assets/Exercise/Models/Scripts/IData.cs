namespace DCLBattle.LaunchMenu
{
    public interface IArmyData 
    {
        void ApplyToModel(IArmyModel model);
    }
    
    public interface IUnitData 
    {
        void ApplyToModel(IUnitModel model);
    }

    public interface IBattleData
    {
        void ApplyToModel(IBattleModel model);
    }
}