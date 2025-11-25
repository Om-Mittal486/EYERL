using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BlinkEffect : MonoBehaviour
{
    [Header("Blink Settings")]
    public Image blinkOverlay;     // Assign the full-screen black Image
    public float blinkSpeed = 0.2f; // Speed of eyelid closing/opening
    public float holdTime = 0.1f;   // Time fully closed before opening

    private Coroutine blinkRoutine;

    // Call this from other scripts: FindObjectOfType<BlinkEffect>().TriggerBlink();
    public void TriggerBlink()
    {
        if (blinkRoutine != null)
            StopCoroutine(blinkRoutine);

        blinkRoutine = StartCoroutine(Blink());
    }

    private IEnumerator Blink()
    {
        // Fade in (close eyes)
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / blinkSpeed;
            SetAlpha(Mathf.Lerp(0, 1, t));
            yield return null;
        }

        yield return new WaitForSeconds(holdTime);

        // Fade out (open eyes)
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / blinkSpeed;
            SetAlpha(Mathf.Lerp(1, 0, t));
            yield return null;
        }

        blinkRoutine = null;
    }

    private void SetAlpha(float a)
    {
        if (blinkOverlay != null)
        {
            Color c = blinkOverlay.color;
            c.a = a;
            blinkOverlay.color = c;
        }
    }
}
