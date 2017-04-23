using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : GameState
{
    public CanvasGroup UIElements;
    public float FadeDuration = 2;
    private bool Pressed;

    protected override void OnStart()
    {
        Game.WelcomeUI.alpha = 1;
    }

    protected override void OnUpdate()
    {
        if (Pressed)
            return;

        if (Input.anyKeyDown)
        {
            Pressed = true;
            StartCoroutine(ProcessUI());
        }
    }

    protected override void OnEnd()
    {
        Game.WelcomeUI.alpha = 0;
    }

    IEnumerator ProcessUI()
    {
        var elapsed = 0.0f;

        while (elapsed < FadeDuration)
        {
            elapsed += Time.deltaTime;
            UIElements.alpha = 1 - (elapsed / FadeDuration);

            yield return null;
        }

        UIElements.alpha = 0;

        Game.SwitchState<GamePlay>();
    }
}
