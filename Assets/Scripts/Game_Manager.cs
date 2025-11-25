using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton reference

    private int score = 0; // Player score

    void Awake()
    {
        // Setup Singleton so it persists in the scene
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Called by CollectiblePoint when collected
    public void AddScore(int amount)
    {
        score += amount;
        Debug.Log("Score: " + score);
    }

    // Optional: get the current score
    public int GetScore()
    {
        return score;
    }

    // Optional: reset score (for restarts or new levels)
    public void ResetScore()
    {
        score = 0;
        Debug.Log("Score reset!");
    }
}
