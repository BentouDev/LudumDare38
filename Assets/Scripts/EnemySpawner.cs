using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public Transform SpawnPoint;

    public Enemy Spawn(GameObject prefab)
    {
        var go = Instantiate(prefab, SpawnPoint.position, Quaternion.identity);
        return go.GetComponent<Enemy>();
    }
}
