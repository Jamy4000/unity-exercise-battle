using TMPro;
using UnityEngine;

namespace DCLBattle.LaunchMenu
{
    public interface IBattleView
    {
        void InjectModel(IBattleModel model);
        void RegisterBattleDataChangedCallback(System.Action<IBattleData> callback);
        void UnregisterBattleDataChangedCallback(System.Action<IBattleData> callback);
    }

    public class BattleView : MonoBehaviour, IBattleView
    {
        [SerializeField] private TMP_Dropdown algorithmDropdown;

        private EnumDropdownWrapper<SearchAlgorithm> enumDropdown;

        private System.Action<IBattleData> _onDataChanged;
        private System.Action<SearchAlgorithm> _onStrategyChangedCallback;

        private void Awake()
        {
            _onStrategyChangedCallback = OnAlgorithmChanged;

            enumDropdown = new EnumDropdownWrapper<SearchAlgorithm>(algorithmDropdown);
            enumDropdown.OnValueChanged += _onStrategyChangedCallback;
        }

        public void InjectModel(IBattleModel battleModel)
        {
            enumDropdown.SetValueWithoutNotify(battleModel.Algorithm);
        }

        public void RegisterBattleDataChangedCallback(System.Action<IBattleData> callback)
        {
            _onDataChanged += callback;
        }

        public void UnregisterBattleDataChangedCallback(System.Action<IBattleData> callback)
        {
            _onDataChanged -= callback;
        }

        private void OnAlgorithmChanged(SearchAlgorithm algorithm)
        {
            var strategyData = new BattleAlgorithmData(algorithm);
            _onDataChanged?.Invoke(strategyData);
        }

        private void OnDestroy()
        {
            enumDropdown.OnValueChanged -= _onStrategyChangedCallback;
            enumDropdown?.Dispose();
        }
    }

    public sealed class BattleAlgorithmData : IBattleData
    {
        private readonly SearchAlgorithm _algorithm;

        public BattleAlgorithmData(SearchAlgorithm algorithm)
        {
            _algorithm = algorithm;
        }

        public void ApplyToModel(IBattleModel model)
        {
            model.Algorithm = _algorithm;
        }
    }
}