using UnityEngine;
using UnityEngine.UI;

namespace DCLBattle.LaunchMenu
{
    // The view for a unit setup in the launch menu
    public interface IUnitView
    {
        void BindPresenter(IUnitPresenter presenter);
        void InjectModel(IUnitModel model);
    }

    public sealed class UnitSliderView : MonoBehaviour, IUnitView
    {
        [SerializeField] private Slider unitCountSlider;
        [SerializeField] private TMPro.TextMeshProUGUI unitNameText;
        [SerializeField] private TMPro.TextMeshProUGUI unitCountText;

        private IUnitPresenter presenter;

        public void BindPresenter(IUnitPresenter presenter)
        {
            this.presenter = presenter;
        }

        public void InjectModel(IUnitModel model)
        {
            int unitCount = model.GetUnitsCount();
            unitCountSlider.SetValueWithoutNotify(unitCount);
            unitCountText.text = unitCount.ToString();
            unitNameText.text = model.GetUnitsName();

            unitCountSlider.onValueChanged.AddListener(OnUnitsCountChanged);
        }
        private void OnUnitsCountChanged(float value)
        {
            presenter.UpdateUnitCount((int)value);
            unitCountText.text = ((int)value).ToString();
        }
    }
}