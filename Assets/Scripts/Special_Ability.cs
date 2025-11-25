using UnityEngine;

public class DisableEnemyOnPickup : MonoBehaviour
{
    [Header("References")]
    public EnemyBehaviour enemy; // drag your enemy here
    public string playerTag = "Player"; // player must have this tag

    [Header("Effect Settings")]
    public float disableDuration = 3f;

    private float disableTimer = 1f;

    private void Update()
    {
        // countdown timer if active
        if (disableTimer > 0f)
        {
            disableTimer -= Time.deltaTime;

            if (disableTimer <= 0f)
            {
                // Re-enable enemy after timer ends
                if (enemy != null)
                {
                    enemy.enabled = true;
                    Debug.Log("? EnemyBehaviour re-enabled!");
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log("?? Player picked up object: " + gameObject.name);

            // Destroy the pickup object
            Destroy(gameObject);

            // Disable enemy and start timer
            if (enemy != null)
            {
                enemy.enabled = false;
                disableTimer = disableDuration;
                Debug.Log("?? EnemyBehaviour disabled for " + disableDuration + "s!");
            }
            else
            {
                Debug.LogWarning("?? Enemy not assigned in " + name);
            }
        }
    }
}
