using System.Linq;
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
        [SerializeField] private Image background;

        [SerializeField] private TMP_Dropdown strategyDropdown;

        [SerializeField] private Slider unitSliderPrefab;
        [SerializeField] private Transform unitsSliderContent;

        private EnumDropdownWrapper<ArmyStrategy> enumDropdown;
        private IArmyPresenter presenter = null;

        private void Awake()
        {
            enumDropdown = new EnumDropdownWrapper<ArmyStrategy>(strategyDropdown);
            enumDropdown.OnValueChanged += OnStrategyChanged;
        }

        public void BindPresenter(IArmyPresenter presenter)
        {
            this.presenter = presenter;
        }

        public void UpdateWithModel(IArmyModel armyModel)
        {
            title.text = armyModel.ArmyName;
            background.color = armyModel.ArmyColor;

            var unitTypes = System.Enum.GetValues(typeof(UnitType)).Cast<UnitType>();
            foreach (UnitType unitType in unitTypes)
            {
                if (armyModel.GetUnitPrefab(unitType) == null)
                    continue;

                int unitCount = armyModel.GetUnitsCount(unitType);
                // TODO Remove hard implementation, use SO instead
                IUnitModel unitModel = new UnitModel(unitCount, unitType.ToString(), unitType);

                IUnitView unitView = Instantiate(unitSliderPrefab.gameObject, unitsSliderContent).GetComponent<IUnitView>();
                unitView.InjectModel(unitModel);

                // TODO Remove hard impl
                IUnitPresenter presenter = new UnitPresenter(unitModel, unitView);
                unitView.BindPresenter(presenter);

                unitModel.OnUnitsCountChanged += OnUnitsCountChanged;
            }
        }

        private void OnUnitsCountChanged(IUnitModel model)
        {
            presenter.UpdateUnit(model.GetUnitsType(), model.GetUnitsCount());
        }

        private void OnStrategyChanged(ArmyStrategy strategy)
        {
            presenter.UpdateStrategy(strategy);
        }

        private void OnDestroy()
        {
            enumDropdown.OnValueChanged -= OnStrategyChanged;
            enumDropdown?.Dispose();
        }
    }
}