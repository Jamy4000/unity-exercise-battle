using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverMenu : MonoBehaviour
{
    public TextMeshProUGUI armyWins;
    public Button goToMenu;

    // TODO use event instead
    public void Populate()
    {
        int armyCount = BattleInstantiator.instance.GetArmiesCount();
        for (int i = 0; i < armyCount; i++)
        {
            if ( BattleInstantiator.instance.GetArmy(i).GetUnits().Count > 0 )
            {
                armyWins.text = $"Army {i} wins!";
                return;
            }
        }
    }

    void Awake()
    {
        goToMenu.onClick.AddListener( GoToMenu );
    }

    void GoToMenu()
    {
        SceneManager.LoadScene(0);
    }
}