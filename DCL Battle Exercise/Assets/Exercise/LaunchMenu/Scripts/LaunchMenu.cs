using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DCLBattle.LaunchMenu
{
    public sealed class LaunchMenu : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private string nextSceneName = "Battle";

        [SerializeField] private Transform armiesScrollViewContent;

        [SerializeField]
        private GameObject armiesInfoPrefab;

        [SerializeField, Interface(typeof(IArmyModel))] 
        private Object[] armyModels;

        private IArmyPresenter[] armyPresenters;

        void Start()
        {
            startButton.onClick.AddListener(OnStart);

            // TODO create army presenter without access to implementation
            armyPresenters = new ArmyPresenter[armyModels.Length];

            for (int i = 0; i < armyModels.Length; i++)
            {
                IArmyModel model = armyModels[i] as IArmyModel;
                
                IArmyView view = Instantiate(armiesInfoPrefab, armiesScrollViewContent).GetComponent<IArmyView>();

                armyPresenters[i] = new ArmyPresenter(model, view);

                view.BindPresenter(armyPresenters[i]);
            }
        }

        void OnStart()
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}