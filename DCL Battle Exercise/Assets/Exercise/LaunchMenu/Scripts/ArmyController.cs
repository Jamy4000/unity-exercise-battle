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
            this._view = view;
            this._view.InjectModel(model);
            this._view.RegisterArmyDataChangedCallback(OnUserInputReceived);
        }

        private void OnUserInputReceived(IArmyData receivedInput)
        {
            receivedInput.ApplyToModel(_model);
        }
    }
}