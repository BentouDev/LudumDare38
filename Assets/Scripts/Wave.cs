using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Wave : MonoBehaviour
{
    [SerializeField]
    public float WaveDuration;

    private float StartTime;

    public class EnemyRuntime
    {
        public List<EnemySpawner> UsedSpawners = new List<EnemySpawner>();
        public List<Enemy> EnemyInstances = new List<Enemy>();
        public EnemyInfo Info;
        public float LastSpawnTime;

        private EnemySpawner PickSpawner()
        {
            if (UsedSpawners.Count == Info.Spawners.Count)
                UsedSpawners.Clear();

            int index = 0;
            do
            {
                index = Random.Range(0, Info.Spawners.Count);

            } while (UsedSpawners.Contains(Info.Spawners[index]));

            EnemySpawner spawner = Info.Spawners[index];

            UsedSpawners.Add(spawner);

            return spawner;
        }

        public bool CanSpawn()
        {
            return Time.time - LastSpawnTime > Info.SpawnDelay 
                && EnemyInstances.Count < Info.MaxAtOnce;
        }

        public void Spawn()
        {
            var enemy = PickSpawner().Spawn(Info.Prefab);

            enemy.Init();

            EnemyInstances.Add(enemy);
            LastSpawnTime = Time.time;
        }

        public EnemyRuntime(EnemyInfo info)
        {
            Info = info;
            LastSpawnTime = 0;
        }
    }

    [System.Serializable]
    public struct EnemyInfo
    {
        [SerializeField]
        public GameObject Prefab;

        [SerializeField]
        public List<EnemySpawner> Spawners;

        [SerializeField]
        public float SpawnDelay;

        [SerializeField]
        public int MaxAtOnce;
    }

    [SerializeField]
    public List<EnemyInfo> Enemies = new List<EnemyInfo>();

    public List<EnemyRuntime> Runtime = new List<EnemyRuntime>();

    public bool AnyEnemies()
    {
        foreach (EnemyRuntime runtime in Runtime)
        {
            if (runtime.EnemyInstances.Any())
                return true;
        }

        return false;
    }

    public bool IsFinished()
    {
        return Time.time - StartTime > WaveDuration && !AnyEnemies();
    }

    public void OnStart()
    {
        Runtime.Clear();
        StartTime = Time.time;

        foreach (EnemyInfo enemy in Enemies)
        {
            Runtime.Add(new EnemyRuntime(enemy));
        }
    }

    public void OnEnd()
    {
        Runtime.Clear();
    }

    public void OnUpdate()
    {
        if (IsFinished())
            return;

        if (Time.time - StartTime < WaveDuration)
        {
            foreach (EnemyRuntime enemy in Runtime)
            {
                if (enemy.CanSpawn())
                {
                    enemy.Spawn();
                }
            }
        }

        foreach (EnemyRuntime runtime in Runtime)
        {
            var toRemove = new List<Enemy>();

            foreach (Enemy instance in runtime.EnemyInstances)
            {
                if (instance.IsAlive())
                    instance.OnUpdate();
                else
                    toRemove.Add(instance);
            }

            foreach (Enemy enemy in toRemove)
            {
                runtime.EnemyInstances.Remove(enemy);
                Destroy(enemy.gameObject);
            }
        }
    }
}
