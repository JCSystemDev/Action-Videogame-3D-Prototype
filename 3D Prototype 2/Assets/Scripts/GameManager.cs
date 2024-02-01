using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameUIManager gameUIManager;
    public Character _playerCharacter;
    public PlayerInput _playerInput;
    private bool gameIsOver;
    

    private void Awake()
    {
        _playerCharacter = GameObject.FindWithTag("Player").GetComponent<Character>();
    }

    public void GameOver()
    {
        gameUIManager.ShowGameOverUI();
    }

    public void GameIsFinished()
    {
        gameUIManager.ShowGameIsFinishedUI();
        _playerInput.inputSwitch = false;
    }

    void Update()
    {
        if (gameIsOver)
        {
            GameOver();
        }

        if (Input.GetButtonDown("Pause"))
        {
            gameUIManager.TogglePauseUI();
        }

        if (_playerCharacter.currentState == Character.CharacterState.Dead)
        {
            gameIsOver = true;

        }
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");

    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
