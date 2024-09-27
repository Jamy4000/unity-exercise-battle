using UnityEngine;
using UnityEngine.UI;

namespace DCLBattle.LaunchMenu
{
    // The view for a unit setup in the launch menu
    public interface IUnitView
    {
        void InjectModel(IArmyModel armyModel, IUnitModel unitModel);
        void RegisterArmyDataChangedCallback(System.Action<IArmyData> callback);
    }

    public sealed class UnitSliderView : MonoBehaviour, IUnitView
    {
        [SerializeField] private Slider _unitCountSlider;
        [SerializeField] private TMPro.TextMeshProUGUI _unitNameText;
        [SerializeField] private TMPro.TextMeshProUGUI _unitCountText;

        private System.Action<IArmyData> _onArmyDataChanged;
        private UnitType _unitType;
        
        public void InjectModel(IArmyModel armyModel, IUnitModel unitModel)
        {
            _unitType = unitModel.UnitType;
            
            int unitCount = armyModel.GetUnitCount(unitModel.UnitType);
            _unitCountSlider.SetValueWithoutNotify(unitCount);
            _unitCountText.text = unitCount.ToString();
            _unitNameText.text = unitModel.UnitName;

            _unitCountSlider.onValueChanged.AddListener(OnUnitsCountChanged);
        }

        public void RegisterArmyDataChangedCallback(System.Action<IArmyData> callback)
        {
            _onArmyDataChanged += callback;
        }

        private void OnUnitsCountChanged(float value)
        {
            int newCount = (int)value;
            _unitCountText.text = newCount.ToString();

            var unitCountData = new ArmyUnitCountData(_unitType, newCount);
            _onArmyDataChanged?.Invoke(unitCountData);
        }
    }

    public sealed class ArmyUnitCountData : IArmyData
    {
        private readonly UnitType _type;
        private readonly int _newCount;

        public ArmyUnitCountData(UnitType type, int newCount)
        {
            _type = type;
            _newCount = newCount;
        }
        
        public void ApplyToModel(IArmyModel model)
        {
            model.SetUnitCount(_type, _newCount);
        }
    }
}