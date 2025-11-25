using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class EndCutsceneTrigger : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string mainMenuSceneName = "MainMenu";

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("EndCollider")) // Tag your end collider as "EndCollider"
        {
            if (videoPlayer != null)
            {
                videoPlayer.Play();
                videoPlayer.loopPointReached += OnVideoEnd;
            }
        }
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}