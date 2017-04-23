using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEnd : GameState
{
    public float Delay = 4;

    private float StarTime;

    protected override void OnStart()
    {
        Game.Instance.LoseUI.alpha = 1;
        StarTime = Time.time;
    }

    protected override void OnUpdate()
    {
        if (Time.time - StarTime > Delay)
            SceneManager.LoadScene(0);
    }
}
