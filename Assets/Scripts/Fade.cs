using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SimpleFade : MonoBehaviour
{
    public float fadeDuration = 1.5f;
    public float fadeInDelay = 0.5f;  // delay before fade-in starts

    private Image fadeImage;
    private bool isFadingOut = false;

    void Awake()
    {
        fadeImage = GetComponent<Image>();
        // Ensure screen starts fully black BEFORE anything is shown
        fadeImage.color = new Color(0, 0, 0, 1);
    }

    void Start()
    {
        // Start fade-in after a short delay
        StartCoroutine(FadeIn());
    }

    System.Collections.IEnumerator FadeIn()
    {
        // Wait before starting fade-in
        yield return new WaitForSeconds(fadeInDelay);

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        fadeImage.color = new Color(0, 0, 0, 0); // Ensure fully transparent
    }

    public void TriggerFadeOut()
    {
        if (!isFadingOut)
            StartCoroutine(FadeOut());
    }

    System.Collections.IEnumerator FadeOut()
    {
        isFadingOut = true;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        fadeImage.color = new Color(0, 0, 0, 1); // Ensure fully black
        SceneManager.LoadScene(1);
    }
}
