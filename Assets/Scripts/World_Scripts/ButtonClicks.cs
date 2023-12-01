using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonClicks : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("Level1");
    }

    public void Exit()
    {
        //Only need this in final build
        Application.Quit();

        //This makes it work in editor
        UnityEditor.EditorApplication.isPlaying = false;
    }
}
