using System.Collections;
using UnityEngine;

public class EnemyDeathExplosion : MonoBehaviour
{
    [SerializeField] GameObject explosion;
    [SerializeField] [Range(0, 3)] float waitTime;
    [SerializeField] EnemyStateMachine stateMachine;
    bool OOOHIMDYING;

    void Update()
    {
        if (OOOHIMDYING) return;
        if (stateMachine.currentState == stateMachine.deathState)
        {
            OOOHIMDYING = true;
            StartCoroutine(WaitAndExplode());
        }
    }

    IEnumerator WaitAndExplode()
    {
        yield return new WaitForSeconds(waitTime);
        Instantiate(explosion, stateMachine.transform.position, Quaternion.identity);
        Destroy(stateMachine.gameObject, 0.2f);
    }
}
