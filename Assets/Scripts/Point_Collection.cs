using UnityEngine;

public class CollectiblePoint : MonoBehaviour
{
    [Header("Point Settings")]
    public int pointValue = 1; // How many points this collectible is worth

    [Header("Effects (Optional)")]
    public GameObject collectEffect; // Particle effect to spawn when collected
    public AudioClip collectSound;   // Sound to play when collected

    private AudioSource audioSource;
    private bool isCollected = false; // Prevents double collection

    void Start()
    {
        // Get or add AudioSource component for sound effects
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && collectSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if player touched the point and it hasn't been collected yet
        if (other.CompareTag("Player") && !isCollected)
        {
            CollectPoint();
        }
    }

    void CollectPoint()
    {
        isCollected = true; // Prevent multiple collections

        // Add point to the manager
        if (PointManager.Instance != null)
        {
            PointManager.Instance.AddPoint(pointValue);
        }
        else
        {
            Debug.LogWarning("PointManager not found! Make sure you have a PointManager in your scene.");
        }

        // Play collect sound
        if (collectSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(collectSound);
        }

        // Spawn collect effect
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, transform.rotation);
        }

        // Hide the collectible (or destroy it)
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        // Destroy the object after a short delay (allows sound to play)
        Destroy(gameObject, collectSound != null ? collectSound.length : 0f);
    }

    // Optional: Make the point rotate for visual appeal
    void Update()
    {
        transform.Rotate(0, 50 * Time.deltaTime, 0); // Rotate around Y-axis
    }
}
