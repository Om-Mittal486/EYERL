using UnityEngine;
using UnityEngine.SceneManagement; // Needed for scene loading

public class NextSceneOnTouch : MonoBehaviour
{
    [Header("Settings")]
    public string playerTag = "Player"; // Tag of the player object

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            // Get current scene index
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

            // Load next scene
            SceneManager.LoadScene(currentSceneIndex + 1);
        }
    }
}
