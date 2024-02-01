using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI_Manager : MonoBehaviour
{
    public void Button_Start()
    {
        SceneManager.LoadScene("Gameplay");
    }
    
    public void Button_Quit()
    {
        Application.Quit();
    }
}
