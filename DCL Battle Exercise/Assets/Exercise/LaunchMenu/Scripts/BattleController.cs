namespace DCLBattle.LaunchMenu
{
    public interface IBattleController
    {
    }

    public sealed class BattleController : IBattleController
    {
        private readonly IBattleModel _model;
        private readonly IBattleView _view;
        private readonly System.Action<IBattleData> _cachedUserInputReceivedCallback;

        public BattleController(IBattleModel model, IBattleView view)
        {
            this._model = model;
            this._view = view;
            this._view.InjectModel(model);

            _cachedUserInputReceivedCallback = OnUserInputReceived;
            this._view.RegisterBattleDataChangedCallback(_cachedUserInputReceivedCallback);
        }

        ~BattleController()
        {
            this._view.UnregisterBattleDataChangedCallback(_cachedUserInputReceivedCallback);
        }

        private void OnUserInputReceived(IBattleData changedData)
        {
            changedData.ApplyToModel(_model);
        }
    }
}