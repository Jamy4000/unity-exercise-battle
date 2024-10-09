using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCLBattle.LaunchMenu
{
    public interface IArmyView
    {
        void InjectModel(IArmyModel model);
        void RegisterArmyDataChangedCallback(System.Action<IArmyData> callback);
        void UnregisterArmyDataChangedCallback(System.Action<IArmyData> callback);
    }

    public class ArmyView : MonoBehaviour, IArmyView
    {
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private Image background;

        [SerializeField] private TMP_Dropdown strategyDropdown;

        [SerializeField] private Slider unitSliderPrefab;
        [SerializeField] private Transform unitsSliderContent;

        private EnumDropdownWrapper<ArmyStrategy> enumDropdown;

        private System.Action<IArmyData> _onDataChanged;
        private System.Action<ArmyStrategy> _onStrategyChangedCallback;
        private System.Action<IArmyData> _cachedOnUnitCountChangedCallback;

        private void Awake()
        {
            _cachedOnUnitCountChangedCallback = OnUnitCountChanged;
            _onStrategyChangedCallback = OnStrategyChanged;

            enumDropdown = new EnumDropdownWrapper<ArmyStrategy>(strategyDropdown);
            enumDropdown.OnValueChanged += _onStrategyChangedCallback;
        }

        public void InjectModel(IArmyModel armyModel)
        {
            title.text = armyModel.ArmyName;
            background.color = armyModel.ArmyColor;
            enumDropdown.SetValueWithoutNotify(armyModel.Strategy);

            for (int unitTypeIndex = 0; unitTypeIndex < IArmyModel.UnitLength; unitTypeIndex++)
            {
                UnitType unitType = (UnitType)unitTypeIndex;
                if (armyModel.GetUnitModel(unitType) == null)
                    continue;

                IUnitModel unitModel = armyModel.GetUnitModel(unitType);
                IUnitView unitView = Instantiate(unitSliderPrefab.gameObject, unitsSliderContent).GetComponent<IUnitView>();
                unitView.InjectModel(armyModel, unitModel);
                
                unitView.RegisterArmyDataChangedCallback(_cachedOnUnitCountChangedCallback);
            }
        }

        public void RegisterArmyDataChangedCallback(System.Action<IArmyData> callback)
        {
            _onDataChanged += callback;
        }

        public void UnregisterArmyDataChangedCallback(System.Action<IArmyData> callback)
        {
            _onDataChanged -= callback;
        }

        private void OnStrategyChanged(ArmyStrategy strategy)
        {
            var strategyData = new ArmyStrategyData(strategy);
            _onDataChanged?.Invoke(strategyData);
        }
        
        private void OnUnitCountChanged(IArmyData newData)
        {
            _onDataChanged?.Invoke(newData);
        }

        private void OnDestroy()
        {
            enumDropdown.OnValueChanged -= _onStrategyChangedCallback;
            enumDropdown?.Dispose();
        }
    }

    public sealed class ArmyStrategyData : IArmyData
    {
        private readonly ArmyStrategy _strategy;

        public ArmyStrategyData(ArmyStrategy strategy)
        {
            _strategy = strategy;
        }
        
        public void ApplyToModel(IArmyModel model)
        {
            model.Strategy = _strategy;
        }
    }
}