namespace DCLBattle.LaunchMenu
{
    public interface IArmyPresenter
    {
        void UpdateUnit(IUnitModel unitModel);

        void UpdateStrategy(ArmyStrategy strategy);
    }

    public sealed class ArmyPresenter : IArmyPresenter
    {
        private readonly IArmyModel model;
        private readonly IArmyView view;

        public ArmyPresenter(IArmyModel model, IArmyView view)
        {
            this.model = model;
            this.view = view;
            this.view.UpdateWithModel(model);
        }

        public void UpdateUnit(IUnitModel unitModel)
        {
            model.SetUnitsCount(unitModel.GetUnitsType(), unitModel.GetUnitsCount());
        }

        public void UpdateStrategy(ArmyStrategy strategy)
        {
            model.Strategy = strategy;
        }
    }
}