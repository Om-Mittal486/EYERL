using UnityEngine;
using UnityEngine.SceneManagement;

public class Quit_Script : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            exit();
        }
    }

    void exit()
    {
        Application.Quit();
        Debug.Log("Game is exiting");
    }
}
