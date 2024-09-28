namespace DCLBattle.LaunchMenu
{
    public interface IArmyController
    {
    }

    public sealed class ArmyController : IArmyController
    {
        private readonly IArmyModel _model;
        private readonly IArmyView _view;

        public ArmyController(IArmyModel model, IArmyView view)
        {
            this._model = model;
            // If the model isn't provided for a specific unit; set its count to 0
            for (int unitIndex = 0; unitIndex < IArmyModel.UnitLength; unitIndex++)
            {
                var type = (UnitType)unitIndex;
                if (model.GetUnitModel(type) == null)
                    model.SetUnitCount(type, 0);
            }

            this._view = view;
            this._view.InjectModel(model);
            this._view.RegisterArmyDataChangedCallback(OnUserInputReceived);
        }

        private void OnUserInputReceived(IArmyData changedData)
        {
            changedData.ApplyToModel(_model);
        }
    }
}