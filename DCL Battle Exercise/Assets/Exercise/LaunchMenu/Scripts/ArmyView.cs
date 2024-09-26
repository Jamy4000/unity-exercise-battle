using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCLBattle.LaunchMenu
{
    public interface IArmyView
    {
        void BindPresenter(IArmyPresenter presenter);
        void UpdateWithModel(IArmyModel model);
    }

    public class ArmyView : MonoBehaviour, IArmyView
    {
        [SerializeField] private TextMeshProUGUI title;

        // TODO This is still hard coded, may do later
        [SerializeField] private Slider warriorsCount;
        [SerializeField] private TextMeshProUGUI warriorsLabel;

        [SerializeField] private Slider archersCount;
        [SerializeField] private TextMeshProUGUI archersLabel;

        [SerializeField] private TMP_Dropdown strategyDropdown;

        private EnumDropdownWrapper<ArmyStrategy> enumDropdown;
        private IArmyPresenter presenter = null;

        private void Awake()
        {
            warriorsCount.onValueChanged.AddListener(OnWarriorsCountChanged);
            archersCount.onValueChanged.AddListener(OnArchersCountChanged);
            enumDropdown = new EnumDropdownWrapper<ArmyStrategy>(strategyDropdown);
            enumDropdown.OnValueChanged += OnStrategyChanged;
        }

        public void BindPresenter(IArmyPresenter presenter)
        {
            this.presenter = presenter;
        }

        public void UpdateWithModel(IArmyModel model)
        {
            var warriorUnitCount = model.GetUnitsCount(UnitType.Warrior);
            warriorsCount.SetValueWithoutNotify(warriorUnitCount);
            warriorsLabel.text = warriorUnitCount.ToString();

            var archerUnitsCount = model.GetUnitsCount(UnitType.Archer);
            archersCount.SetValueWithoutNotify(archerUnitsCount);
            archersLabel.text = archerUnitsCount.ToString();

            enumDropdown.SetValueWithoutNotify(model.Strategy);
        }

        // TODO shouldn't be warrior only
        private void OnWarriorsCountChanged(float value)
        {
            presenter.UpdateUnit(UnitType.Warrior, (int)value);
            warriorsLabel.text = ((int)value).ToString();
        }

        // TODO shouldn't be archer only
        private void OnArchersCountChanged(float value)
        {
            presenter.UpdateUnit(UnitType.Archer, (int)value);
            archersLabel.text = ((int)value).ToString();
        }

        private void OnStrategyChanged(ArmyStrategy strategy)
        {
            presenter?.UpdateStrategy(strategy);
        }

        private void OnDestroy()
        {
            enumDropdown.OnValueChanged -= OnStrategyChanged;
            enumDropdown?.Dispose();
        }
    }
}