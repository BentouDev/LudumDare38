using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Game : MonoBehaviour
{
    [Header("References")]
    public Transform WorldRoot;
    public Pawn Pawn;
    public ActionCamera ActionCamera;
    public Transform Nexus;

    public CanvasGroup WelcomeUI;
    public CanvasGroup LoseUI;

    [Header("States")]
    public GameState FirstState;

    [HideInInspector]
    public List<GameState> AllStates;
    public GameState CurrentState { get; private set; }
    
    // Singleton
    public static Game Instance { get; private set; }

    void Start()
    {
        Init();
    }

    public bool IsPlaying()
    {
        return CurrentState is GamePlay;
    }

    protected void Init()
    {
        Instance = this;

        if (!WorldRoot)
        {
            var go = GameObject.FindGameObjectWithTag("WorldRoot");
            WorldRoot = go ? go.transform : null;
        }

        if (!Pawn)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            Pawn = go ? go.GetComponent<Pawn>() : null;
        }

        if (!ActionCamera)
        {
            var go = GameObject.FindGameObjectWithTag("MainCamera");
            ActionCamera = go ? go.GetComponent<ActionCamera>() : null;
        }

        AllStates = FindObjectsOfType<GameState>().ToList();
        foreach (GameState state in AllStates)
        {
            state.Init(this);
        }

        CurrentState = FirstState;
        
        if(CurrentState)
            CurrentState.DoStart();
    }

    void FixedUpdate()
    {
        if (CurrentState)
            CurrentState.DoUpdate();
    }

    public void SwitchState<T>() where T : GameState
    {
        SwitchState(AllStates.FirstOrDefault(s => s is T));
    }

    public void SwitchState(GameState newState)
    {
        if(CurrentState)
            CurrentState.DoEnd();

        CurrentState = newState;

        if(CurrentState)
            CurrentState.DoStart();
    }
}
