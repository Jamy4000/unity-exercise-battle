using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;

namespace DCLBattle.Battle
{
    public sealed class GameOverMenu : MonoBehaviour, ISubscriber<AllianceWonEvent>
    {
        [SerializeField]
        private TextMeshProUGUI _armyWins;
        [SerializeField]
        public Button _goToMenu;

        void Awake()
        {
            _goToMenu.onClick.AddListener(GoToMenu);
            gameObject.SetActive(false);
            MessagingSystem<AllianceWonEvent>.Subscribe(this);
        }

        private void OnDestroy()
        {
            MessagingSystem<AllianceWonEvent>.Unsubscribe(this);
        }

        void GoToMenu()
        {
            SceneManager.LoadScene(0);
        }

        public void OnEvent(AllianceWonEvent evt)
        {
            var armiesHolder = UnityServiceLocator.ServiceLocator.Global.Get<IArmiesHolder>();

            List<string> winnersList = new(armiesHolder.ArmiesCount);

            for (int i = 0; i < armiesHolder.ArmiesCount; i++)
            {
                IArmyModel armyModel = armiesHolder.GetArmy(i).Model;
                if (armyModel.AllianceID == evt.AllianceID)
                {
                    winnersList.Add(armyModel.ArmyName);
                }
            }

            string concatenatedNames = string.Empty;
            for (int i = 0; i < winnersList.Count; i++)
            {
                if (i == 0)
                {
                    concatenatedNames += winnersList[i];
                }
                else if (i == winnersList.Count - 1)
                {
                    concatenatedNames += $" and {winnersList[i]}";
                }
                else
                {
                    concatenatedNames += $", {winnersList[i]}";
                }
            }

            _armyWins.text = $"The Armies of {concatenatedNames} won!";
            gameObject.SetActive(true);
        }
    }
}