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
        [SerializeField]
        private TextMeshProUGUI _armyWins;
        [SerializeField]
        public Button _goToMenu;

        void Awake()
        {
            _goToMenu.onClick.AddListener(GoToMenu);

            gameObject.SetActive(false);
        }

        private void Start()
        {
            
        }

        // TODO use event instead
        public void OnArmyWon(Army winner)
        {
            _armyWins.text = $"The Armies of {winner.Model.ArmyName} wins!";
        }

        void GoToMenu()
        {
            SceneManager.LoadScene(0);
        }
    }
}