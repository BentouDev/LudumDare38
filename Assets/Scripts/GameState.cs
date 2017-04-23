using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameState : MonoBehaviour
{
    protected Game Game;

    public void Init(Game game)
    {
        Game = game;
    }

    public void DoStart()
    {
        OnStart();
    }

    public void DoUpdate()
    {
        OnUpdate();
    }

    public void DoEnd()
    {
        OnEnd();
    }

    protected virtual void OnStart()
    { }

    protected virtual void OnUpdate()
    { }

    protected virtual void OnEnd()
    { }
}
