using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class ParallaxCamera : MonoBehaviour
{
    public delegate void ParallaxCameraDelegate(float deltaMovement);
    public ParallaxCameraDelegate onCameraTranslate;

    private float oldPosition;

    void Start()
    {
        oldPosition = transform.position.x;

    }

    void Update()
    {
        // Check if the current scene is the Main Menu
        if (SceneManager.GetActiveScene().name == "Main Menu")
        {
            // Trigger constant parallax effect
            if (onCameraTranslate != null)
            {
                float delta = 2f;
                onCameraTranslate(delta);
            }
        }

      
            if (transform.position.x != oldPosition)
            {
                if (onCameraTranslate != null)
                {
                    float delta = oldPosition - transform.position.x;
                    onCameraTranslate(delta);
                }
            }

            oldPosition = transform.position.x;
        
    }


    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset old position when a new scene is loaded
        oldPosition = transform.position.x;
    }

    void OnDestroy()
    {
        // Unsubscribe from the scene loaded event to prevent memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}

// Inspired by: https://www.youtube.com/watch?v=MEy-kIGE-lI