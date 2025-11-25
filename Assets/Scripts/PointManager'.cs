using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
public class PointManager : MonoBehaviour
{
    public GameStats gameStats;
    public TMP_Text timeText;    // Drag TMP for time
    private float startTime;


    [Header("Point Settings")] // <- fixed closing bracket

    [Header("References")]
    public TMP_Text scoreText;   // Drag your TMP object here
    private int totalPoints = 0; // <- added missing variable


    [Header("UI (Optional)")]
    public Text pointsText; // Drag your UI Text component here if you want to display points

    // Static reference so other scripts can easily access it
    public static PointManager Instance;


    void Awake()
    {
        // Singleton pattern - ensures only one PointManager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keeps points between scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        startTime = Time.time;
        gameStats.totalPoints = 0; // initialize
        UpdatePointsDisplay();
    }
    private void Update()
    {
        scoreText.text = gameStats.totalPoints.ToString();
        float time = Time.time;
        int minutes = Mathf.FloorToInt(time / 60F);
        int seconds = Mathf.FloorToInt(time % 60F);
        timeText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    // Method to add points
    public void AddPoint(int pointValue = 1)
    {
        gameStats.totalPoints += pointValue;
        UpdatePointsDisplay();

        // Optional: Play sound effect or particle effect here
        Debug.Log("Points collected: " + totalPoints);
    }

    // Method to get current points
    public int GetPoints()
    {
        return totalPoints;
    }

    // Method to reset points (useful for new game)
    public void ResetPoints()
    {
        totalPoints = 0;
        UpdatePointsDisplay();
    }

    // Update the UI display
    void UpdatePointsDisplay()
    {
        if (pointsText != null)
        {
            pointsText.text = "Points: " + totalPoints.ToString();
        }
    }
}
