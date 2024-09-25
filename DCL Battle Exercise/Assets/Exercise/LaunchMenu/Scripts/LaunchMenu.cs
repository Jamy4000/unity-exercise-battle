using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DCLBattle.LaunchMenu
{
    public sealed class LaunchMenu : MonoBehaviour
    {
        [System.Serializable]
        private struct ArmyData
        {
            [SerializeField] private ArmyModelSO armyModel;
            public ArmyModelSO GetModel() => armyModel;
        
            [SerializeField] private ArmyView armyView;
            public ArmyView GetView() => armyView;
        }
    
        [SerializeField] private Button startButton;
        [SerializeField] private int nextSceneIndex = 1;
        [SerializeField] private ArmyData[] armyData;

        private ArmyPresenter[] armyPresenters;

        void Start()
        {
            startButton.onClick.AddListener(OnStart);

            armyPresenters = new ArmyPresenter[armyData.Length];
            for (int i = 0; i < armyData.Length; i++)
            {
                var data = armyData[i];
                armyPresenters[i] = new ArmyPresenter(data.GetModel(), data.GetView());
                data.GetView().BindPresenter(armyPresenters[i]);
            }
        }

        void OnStart()
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
    }
}