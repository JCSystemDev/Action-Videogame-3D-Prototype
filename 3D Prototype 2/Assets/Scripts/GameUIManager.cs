using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    public GameManager gm;
    public TMPro.TextMeshProUGUI coinText;
    public Slider healthSlider;
    public GameObject UI_Pause;
    public GameObject UI_GameOver;
    public GameObject UI_GameIsFinished;

    private enum GameUI_State
    {
        Gameplay, Pause, GameOver, GameIsFinished
    }

    GameUI_State currentState;

    private void Start()
    {
        SwitchUIState(GameUI_State.Gameplay);
    }

    void Update()
    {
        healthSlider.value = gm._playerCharacter.GetComponent<Health>().currentHealthPercentage;
        coinText.text = gm._playerCharacter.coinsAmount.ToString();
    }

    private void SwitchUIState(GameUI_State state)
    {
        UI_Pause.SetActive(false);
        UI_GameOver.SetActive(false);
        UI_GameIsFinished.SetActive(false);

        Time.timeScale = 1;

        switch (state)
        {
            case GameUI_State.Gameplay:
                break;
            case GameUI_State.Pause:
                Time.timeScale = 0;
                UI_Pause.SetActive(true);
                break;
            case GameUI_State.GameOver:
                UI_GameOver.SetActive(true);
                break;
            case GameUI_State.GameIsFinished:
                UI_GameIsFinished.SetActive(true);
                break;
        }

        currentState = state;
    }

    public void TogglePauseUI()
    {
        if (currentState == GameUI_State.Gameplay)
        {
            SwitchUIState(GameUI_State.Pause);
        }
        else if (currentState == GameUI_State.Pause)
        {
            SwitchUIState(GameUI_State.Gameplay);
        }
    }

    public void Button_MainMenu()
    {
        gm.ReturnToMainMenu();
    }

    public void Button_Restart()
    {
        gm.Restart();
    }

    public void ShowGameOverUI()
    {
        SwitchUIState(GameUI_State.GameOver);
    }

    public void ShowGameIsFinishedUI()
    {
        SwitchUIState(GameUI_State.GameIsFinished);
    }
}
