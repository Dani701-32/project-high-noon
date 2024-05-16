using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject enemy;
    [SerializeField] float maxRandomTime;
    [SerializeField] float minRandomTime;

    List<GameObject> created = new List<GameObject>();
    
    void Start()
    {
        StartCoroutine("CreateEnemy");
    }

    IEnumerator CreateEnemy()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minRandomTime, maxRandomTime));
            if (created.Count < 5)
            {
                GameObject newEnemy = Instantiate(enemy, transform.position, Quaternion.identity);
                created.Add(newEnemy);
            }
        }
    }
}
