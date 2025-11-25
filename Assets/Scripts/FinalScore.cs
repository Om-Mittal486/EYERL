using UnityEngine;
using TMPro;

public class ResultDisplay : MonoBehaviour
{
    public TMP_Text pointsText;   // assign TMP for points
    public GameStats gameStats;   // assign the same ScriptableObject

    void Start()
    {
        // Display points
        pointsText.text = "Total Score: " + gameStats.totalPoints;

        // Display time in MM:SS format
        float totalTime = gameStats.totalTime;
    }
}
