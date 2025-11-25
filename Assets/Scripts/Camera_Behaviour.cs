using UnityEngine;
using UnityEngine.SceneManagement;

public class FPVCamera : MonoBehaviour
{
    [Header("Mouse & Joystick Settings")]
    public float sensitivity = 200f;
    public bool invertY = false;
    public float joystickDeadZone = 0.2f;

    [Header("Camera Limits")]
    public float minYAngle = -90f;
    public float maxYAngle = 90f;

    [Header("Options")]
    public bool lockCursor = true;

    private float xRotation = 0f;
    private Transform playerBody;

    void Start()
    {
        playerBody = transform.parent;

        if (playerBody == null)
        {
            Debug.LogError("FPV Camera must be a child of the player!");
            return;
        }

        if (lockCursor) LockCursor();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) ToggleCursorLock();

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            HandleLook();
        }
    }

    void HandleLook()
    {
        // Mouse input
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Joystick input (Right Stick)
        float joyX = Input.GetAxis("RightStickHorizontal");
        float joyY = Input.GetAxis("RightStickVertical");

        // Deadzone filtering
        if (Mathf.Abs(joyX) < joystickDeadZone) joyX = 0f;
        if (Mathf.Abs(joyY) < joystickDeadZone) joyY = 0f;

        // Combine inputs
        float finalX = (mouseX + joyX) * sensitivity * Time.deltaTime;
        float finalY = (mouseY + joyY) * sensitivity * Time.deltaTime;

        if (invertY) finalY = -finalY;

        // Rotate player body horizontally
        playerBody.Rotate(Vector3.up * finalX);

        // Rotate camera vertically
        xRotation -= finalY;
        xRotation = Mathf.Clamp(xRotation, minYAngle, maxYAngle);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    void ToggleCursorLock()
    {
        if (Cursor.lockState == CursorLockMode.Locked) UnlockCursor();
        else LockCursor();
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 3) // Disable in Scene 3
        {
            UnlockCursor();
            enabled = false;
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
