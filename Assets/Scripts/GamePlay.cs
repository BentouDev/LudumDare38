using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GamePlay : GameState
{
    public WaveUI WaveUI;

    public List<Wave> Waves;
    
    private Wave CurrentWave;

    private int CurrentWaveIndex;

    protected override void OnStart()
    {
        WaveUI.Init();
        Game.Nexus.Init();
        Game.Pawn.Init();
        Game.ActionCamera.Init();

        Waves = FindObjectsOfType<Wave>().OrderBy(w => w.Priority).ToList();

        if (Waves.Any())
        {
            CurrentWaveIndex = 0;
            CurrentWave = Waves.First();
            StartNewWave();
        }
        else
        {
            CurrentWave = null;
        }
    }

    void StartNewWave()
    {
        WaveUI.OnNewWave(CurrentWaveIndex);
        CurrentWave.OnStart();
    }

    protected override void OnUpdate()
    {
        if ((Game.Pawn  && !Game.Pawn.IsAlive())
        ||  (Game.Nexus && !Game.Nexus.IsAlive()))
        {
            Game.SwitchState<GameEnd>();
        }
        else
        {
            UpdateWave();

            if(Game.Pawn)
                Game.Pawn.OnUpdate();
        }
    }

    void UpdateWave()
    {
        if (CurrentWave)
        {
            if (CurrentWave.IsFinished())
            {
                CurrentWave.OnEnd();

                CurrentWaveIndex++;
                if (CurrentWaveIndex < Waves.Count)
                {
                    CurrentWave = Waves[CurrentWaveIndex];
                    StartNewWave();
                }
                else
                {
                    CurrentWave = null;
                }
            }
            else
            {
                CurrentWave.OnUpdate();
            }
        }
    }
}
