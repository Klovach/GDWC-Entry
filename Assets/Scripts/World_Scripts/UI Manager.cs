using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI pointsText;
    public TextMeshProUGUI healthText;

    // Singleton pattern for easy access, just like the SoundManager. 
    private static UIManager _instance;
    public static UIManager Instance => _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UpdatePointsDisplay(int points)
    {
        pointsText.text = "Points: " + points.ToString();
    }

    public void UpdateHealthDisplay(int health)
    {
        pointsText.text = "Health: " + health.ToString();
    }
}