using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class HitScreenEffect : MonoBehaviour
{
    [Range(0f, 1f)]
    public float intensity = 0.25f;

    public float fadeDuration = 0.5f;

    private Image overlay;
    private Coroutine effect;

    private void Awake()
    {
        overlay = GetComponent<Image>();
        overlay.raycastTarget = false;
        SetAlpha(0f);
    }

    public void Flash()
    {
        if (effect != null)
            StopCoroutine(effect);

        effect = StartCoroutine(Fade());
    }

    private IEnumerator Fade()
    {
        SetAlpha(intensity);

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            // Oyun durdurulsa bile efekt tamamlanır.
            elapsed += Time.unscaledDeltaTime;

            float progress = fadeDuration > 0f
                ? Mathf.Clamp01(elapsed / fadeDuration)
                : 1f;

            SetAlpha(Mathf.Lerp(intensity, 0f, progress));
            yield return null;
        }

        SetAlpha(0f);
        effect = null;
    }

    private void SetAlpha(float alpha)
    {
        Color color = overlay.color;
        color.a = alpha;
        overlay.color = color;
    }
}