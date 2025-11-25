using UnityEngine;
using System.Collections;

public class EnemyBehaviour : MonoBehaviour
{
    [Header("References")]
    public Transform player;             // Player transform
    public Transform playerCamera;       // Player camera (child of player)
    public Transform teleportTarget;     // Where ghost teleports after staring
    public AudioSource jumpscareAudio;   // AudioSource for jumpscare sound

    [Header("Player Control")]
    public MonoBehaviour playerMovementScript; // Reference to player movement script
    public float cameraShakeDuration = 3f;     // Duration of camera shake
    public float shakeIntensity = 0.1f;        // Intensity of camera shake

    [Header("Tracking Settings")]
    public float trackingInterval = 1f;  // Time between choosing new target
    public float moveTime = 1.5f;        // Time to move toward target
    public float minOffset = -0.5f;      // Random offset range
    public float maxOffset = 0.5f;

    [Header("Stare Settings")]
    public float detectRadius = 0.1f;    // Distance to trigger stare
    public float stareDuration = 2f;     // How long ghost stares before teleport

    [Header("Jumpscare Settings")]
    public float jumpscareDuration = 2f; // How long ghost stays in front
    public float jumpscareDistance = 1.5f;

    // --- Private state ---
    private Vector3 startPos;
    private Vector3 targetPos;
    private float moveTimer;
    private float trackingTimer;

    private bool isStaring = false;
    private float stareTimer;

    private bool isJumpscaring = false;
    private float jumpscareTimer;

    // Camera shake variables
    private Vector3 originalCameraPos;
    private Quaternion originalCameraRot;
    private bool isPlayerControlDisabled = false;
    private Coroutine cameraShakeCoroutine;

    private void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        // Auto-find player camera if not assigned
        if (playerCamera == null && player != null)
        {
            playerCamera = player.GetComponentInChildren<Camera>().transform;
        }

        startPos = transform.position;
        targetPos = transform.position;
        trackingTimer = trackingInterval;

        // Store original camera position relative to player
        if (playerCamera != null)
        {
            originalCameraPos = playerCamera.localPosition;
            originalCameraRot = playerCamera.localRotation;
        }
    }

    private void Update()
    {

        if (isJumpscaring)
        {
            jumpscareTimer -= Time.deltaTime;
            if (jumpscareTimer <= 0f)
            {
                EndJumpscare();
            }
            return; // Skip everything else during jumpscare
        }

        // --- Handle staring ---
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (!isStaring && distanceToPlayer <= detectRadius)
        {
            StartStare();
        }

        if (isStaring)
        {
            stareTimer -= Time.deltaTime;
            FacePlayer();

            if (stareTimer <= 0f)
            {
                EndStare();
            }
            return; // Skip normal movement while staring
        }

        // --- Normal tracking ---
        trackingTimer -= Time.deltaTime;
        if (trackingTimer <= 0f)
        {
            SetNewTarget();
            trackingTimer = trackingInterval;
        }

        moveTimer += Time.deltaTime / moveTime;
        transform.position = Vector3.Lerp(startPos, targetPos, moveTimer);

        FacePlayer();
    }

    // ----------------- Behaviors -----------------

    private void StartStare()
    {
        isStaring = true;
        stareTimer = stareDuration;
        DisablePlayerControl();
        ForceCameraLookAtGhost();
        StartCameraShake();
        PlayJumpscareSound();
        Debug.Log("👀 Ghost started staring!");
    }

    private void EndStare()
    {
        isStaring = false;
        EnablePlayerControl();
        StopCameraShake();

        if (teleportTarget != null)
        {
            transform.position = teleportTarget.position;
            Debug.Log("⚡ Ghost teleported after staring!");
        }
        else
        {
            Debug.LogWarning("❌ No teleport target set!");
        }

        FacePlayer();
        ResetTracking();
    }

    public void TriggerJumpscare()
    {
        // Prevent starting a new jumpscare if one is already active
        if (isJumpscaring || isStaring) return;

        if (player == null) return;

        // Appear in front of player
        Vector3 frontPos = player.position + player.forward * jumpscareDistance;
        frontPos.y = player.position.y; // Keep same height

        transform.position = frontPos;
        FacePlayer();

        isJumpscaring = true;
        jumpscareTimer = jumpscareDuration;
        DisablePlayerControl();
        ForceCameraLookAtGhost();
        StartCameraShake();
        PlayJumpscareSound();
        Debug.Log("👻 Ghost jumpscare triggered!");
    }

    private void EndJumpscare()
    {
        isJumpscaring = false;
        EnablePlayerControl();
        StopCameraShake();

        // After jumpscare, teleport ghost away (to teleportTarget if assigned)
        if (teleportTarget != null)
        {
            transform.position = teleportTarget.position;
            Debug.Log("👻 Jumpscare ended, ghost teleported away!");
        }

        ResetTracking();
    }

    // ----------------- Player Control Methods -----------------

    private void DisablePlayerControl()
    {
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = false;
        }

        // Also disable mouse look by locking cursor (optional)
        Cursor.lockState = CursorLockMode.Locked;
        isPlayerControlDisabled = true;
    }

    private void EnablePlayerControl()
    {
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true;
        }

        isPlayerControlDisabled = false;
    }

    private void ForceCameraLookAtGhost()
    {
        if (playerCamera == null) return;

        // Calculate direction from camera to ghost
        Vector3 directionToGhost = (transform.position - playerCamera.position).normalized;

        // Make camera look at ghost
        Quaternion targetRotation = Quaternion.LookRotation(directionToGhost);
        playerCamera.rotation = targetRotation;
    }

    private void StartCameraShake()
    {
        if (cameraShakeCoroutine != null)
        {
            StopCoroutine(cameraShakeCoroutine);
        }
        cameraShakeCoroutine = StartCoroutine(CameraShakeCoroutine());
    }

    private void StopCameraShake()
    {
        if (cameraShakeCoroutine != null)
        {
            StopCoroutine(cameraShakeCoroutine);
            cameraShakeCoroutine = null;
        }

        // Reset camera to original position
        if (playerCamera != null)
        {
            playerCamera.localPosition = originalCameraPos;
            playerCamera.localRotation = originalCameraRot;
        }
    }

    private IEnumerator CameraShakeCoroutine()
    {
        float elapsed = 0f;

        while (elapsed < cameraShakeDuration)
        {
            // Continue looking at ghost during shake
            if (isStaring || isJumpscaring)
            {
                ForceCameraLookAtGhost();
            }

            // Apply random shake offset to camera position
            Vector3 randomOffset = Random.insideUnitSphere * shakeIntensity;
            randomOffset.z = 0; // Don't shake forward/backward too much

            playerCamera.localPosition = originalCameraPos + randomOffset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure camera returns to original position
        playerCamera.localPosition = originalCameraPos;
    }

    // ----------------- Helpers -----------------

    private void SetNewTarget()
    {
        startPos = transform.position;
        moveTimer = 0f;

        Vector3 offset = new Vector3(
            Random.Range(minOffset, maxOffset),
            0f,
            Random.Range(minOffset, maxOffset)
        );

        targetPos = player.position + offset;
    }

    private void ResetTracking()
    {
        startPos = transform.position;
        targetPos = transform.position;
        trackingTimer = trackingInterval;
        moveTimer = 0f;
    }

    private void FacePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0; // Keep rotation horizontal
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    private void PlayJumpscareSound()
    {
        if (jumpscareAudio != null)
            jumpscareAudio.Play();
    }

    // Clean up on disable
    private void OnDisable()
    {
        if (isPlayerControlDisabled)
        {
            EnablePlayerControl();
            StopCameraShake();
        }
    }
}