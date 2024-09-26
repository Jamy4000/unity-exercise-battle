namespace DCLBattle.LaunchMenu
{
    public interface IUnitPresenter
    {
        void UpdateUnitCount(int unitCount);
    }

    public sealed class UnitPresenter : IUnitPresenter
    {
        private readonly IUnitModel model;
        private readonly IUnitView view;

        public UnitPresenter(IUnitModel model, IUnitView view)
        {
            this.model = model;
            this.view = view;
            this.view.InjectModel(model);
        }

        public void UpdateUnitCount(int unitCount)
        {
            model.SetUnitsCount(unitCount);
        }
    }
}