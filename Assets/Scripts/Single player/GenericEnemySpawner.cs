using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class GenericEnemySpawner : MonoBehaviour
{
    [Space]
    public bool spawning;
    public GameObject[] enemiesToSpawn;
    public Transform spawnPosition;
    [Header("Timing")]
    [SerializeField] bool immediateSpawnOnCreation;
    [SerializeField] float initialSpawnWaitTime = 1f;
    [SerializeField] float spawnTimeRandomizationRange;

    bool waiting;

    void Start()
    {
        if (!spawnPosition)
            spawnPosition = transform;
        spawnTimeRandomizationRange = Mathf.Max(0, spawnTimeRandomizationRange);
        if (enemiesToSpawn.Length == 0)
        {
            Debug.LogError("Spawner de nome " + gameObject.name + " não tem objetos a serem criados!");
            enabled = false;
        }
    }

    void Update()
    {
        if (!spawning || waiting) return;
        waiting = true;
        StartCoroutine(SpawnEntity());
    }

    IEnumerator SpawnEntity()
    {
        if (immediateSpawnOnCreation)
        {
            immediateSpawnOnCreation = false;
            yield return new WaitForEndOfFrame();
        }
        else
        {
            float randomness = spawnTimeRandomizationRange > 0
                ? Random.Range(0, spawnTimeRandomizationRange)
                : 0;
            yield return new WaitForSeconds(initialSpawnWaitTime + randomness);   
        }
        int rand = Random.Range(0, enemiesToSpawn.Length);
        Instantiate(enemiesToSpawn[rand], spawnPosition.position, spawnPosition.rotation, gameObject.transform);
        waiting = false;
    }
}