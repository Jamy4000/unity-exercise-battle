using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DCLBattle.Battle
{
    public sealed class GameOverMenu : MonoBehaviour
    {
        public TextMeshProUGUI armyWins;
        public Button goToMenu;

        // TODO use event instead
        public void Populate()
        {
            int armyCount = BattleInstantiator.Instance.GetArmiesCount();
            for (int armyIndex = 0; armyIndex < armyCount; armyIndex++)
            {
                if (BattleInstantiator.Instance.GetArmy(armyIndex).RemainingUnitsCount > 0)
                {
                    armyWins.text = $"Army {armyIndex} wins!";
                    return;
                }
            }
        }

        void Awake()
        {
            goToMenu.onClick.AddListener(GoToMenu);
        }

        void GoToMenu()
        {
            SceneManager.LoadScene(0);
        }
    }
}