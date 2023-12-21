using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonClicks : MonoBehaviour
{
    public void StartGame()
    {
        // All counting in an array starts at zero. However, the scene count in build settings does not, so we subtract by one. 
        int nextSceneIndex = 0; 

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
            SoundManager.Instance.PlayLevelMusic(nextSceneIndex);
        }
        else
        {
            Debug.Log("No more levels past this point!");
        }
    }

    public void Exit()
    {
        //Only need this in final build
        Application.Quit();

        //This makes it work in editor
        UnityEditor.EditorApplication.isPlaying = false;
    }
}
