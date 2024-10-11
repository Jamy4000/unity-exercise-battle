using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DCLBattle.LaunchMenu
{
    public sealed class LaunchMenu : MonoBehaviour
    {
        [SerializeField] private Button _startButton;
        [SerializeField] private string _nextSceneName = "Battle";

        [SerializeField] private Transform _armiesScrollViewContent;

        [SerializeField]
        private GameObject _armiesInfoPrefab;

        [SerializeField, Interface(typeof(IArmyModel))] 
        private Object[] _armyModels;

        [SerializeField, Interface(typeof(IBattleModel))]
        private Object _battleModel;

        [SerializeField, Interface(typeof(IBattleView))]
        private Object _battleView;

        // Just in order to keep the controllers alive 
        private IArmyController[] _armyControllers;
        private IBattleController _battleController;

        void Start()
        {
            _startButton.onClick.AddListener(OnStart);

            _battleController = new BattleController(_battleModel as IBattleModel, _battleView as IBattleView);

            _armyControllers = new IArmyController[_armyModels.Length];
            for (int i = 0; i < _armyModels.Length; i++)
            {
                IArmyModel model = _armyModels[i] as IArmyModel;
                
                IArmyView view = Instantiate(_armiesInfoPrefab, _armiesScrollViewContent).GetComponent<IArmyView>();

                _armyControllers[i] = new ArmyController(model, view);
            }
        }

        void OnStart()
        {
            SceneManager.LoadScene(_nextSceneName);
        }
    }
}