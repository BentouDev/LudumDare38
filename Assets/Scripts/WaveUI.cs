using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaveUI : MonoBehaviour
{
    public Text WaveTitle;
    public CanvasGroup Master;
    public float FadeDuration;
    public float ShowAnimDuration;

    public static WaveUI Instance;

    public void Init()
    {
        Instance = this;
    }

    public void OnNewWave(int index)
    {
        WaveTitle.text = "Wave " + (index + 1);
        StartCoroutine(AnimateNewWave());
    }

    public void Show()
    {
        StartCoroutine(Animate(FadeDuration, 0));
    }

    public void Hide()
    {
        StartCoroutine(Animate(FadeDuration, 1));
    }

    IEnumerator AnimateNewWave()
    {
        Show();

        yield return new WaitForSeconds(ShowAnimDuration);

        Hide();
    }

    IEnumerator Animate(float duration, float startValue)
    {
        var elapsed = 0.0f;

        while (elapsed < duration)
        {
            Master.alpha = Mathf.Abs(startValue - (elapsed / duration));
            elapsed += Time.deltaTime;

            yield return null;
        }

        Master.alpha = 1 - startValue;
    }
}
