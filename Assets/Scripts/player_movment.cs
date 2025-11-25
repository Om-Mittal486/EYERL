using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Control")]
    public MonoBehaviour playerController; // assign your FPS controller script here

    private void LockPlayer(bool state)
    {
        if (playerController != null)
            playerController.enabled = !state; // disable movement/rotation when locked
    }

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    void Start()
    {
        // Get the Character Controller component
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Check if grounded using Character Controller
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small negative value to keep player grounded
        }

        // Get input from WASD or Arrow Keys
        float horizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right arrows
        float vertical = Input.GetAxis("Vertical");     // W/S or Up/Down arrows

        // Calculate movement direction relative to player's rotation (for FPV)
        Vector3 direction = transform.right * horizontal + transform.forward * vertical;

        // Move the player
        if (direction.magnitude >= 0.1f)
        {
            controller.Move(direction * moveSpeed * Time.deltaTime);
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        // Apply vertical movement (gravity and jumping)
        controller.Move(velocity * Time.deltaTime);
    }
}