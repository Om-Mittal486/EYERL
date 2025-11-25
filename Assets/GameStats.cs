using UnityEngine;

[CreateAssetMenu(fileName = "GameStats", menuName = "ScriptableObjects/GameStats")]
public class GameStats : ScriptableObject
{
    public int totalPoints;
    public float totalTime; // in seconds
}
