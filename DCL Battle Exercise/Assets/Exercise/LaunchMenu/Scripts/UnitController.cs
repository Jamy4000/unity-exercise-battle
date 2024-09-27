namespace DCLBattle.LaunchMenu
{
    public interface IUnitController
    {
    }

    public sealed class UnitController : IUnitController
    {
        private readonly IUnitModel model;
        private readonly IUnitView view;

        public UnitController(IUnitModel model, IUnitView view)
        {
            this.model = model;
            this.view = view;
        }
    }
}