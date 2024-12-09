using System.Collections;
using UnityEngine;

public class EnBombTossState : State
{
    [Header("References")]
    [SerializeField] EnemyStateMachine stateMachine;
    [SerializeField] State followupState;

    [Space]
    [Header("State data")]
    [SerializeField] ShootableBomb bomb;
    [SerializeField] Transform bombSpawnLocation;
    [SerializeField] LayerMask bombCollisionMask;
    [SerializeField] float attackDuration;
    [SerializeField] float bombTossStartup;
    [Space]
    [Space]
    [Space]
    [SerializeField] bool setNewWaitTimeOnEnd;
    [SerializeField] float newWaitTime;
    [SerializeField] float firingAngle = 45.0f;
    [SerializeField] float gravity = 9.8f;

    float duration;
    Transform mainBody;
    GameObject target;

    void Start()
    {
        mainBody = stateMachine.transform;
        newWaitTime = Mathf.Max(0, newWaitTime);
        attackDuration = Mathf.Max(0, attackDuration);
        attackDuration += Mathf.Max(bombTossStartup, 0);

        if (setNewWaitTimeOnEnd && followupState.GetType() != typeof(EnWaitingState))
            Debug.LogWarning("Novo estado após ataque não é EnWaitingState, mas o estado ainda está passando um novo tempo de espera ao objeto.");
    }

    public override void SwitchIntoState()
    {
        StartAnim(stateMachine);
        // Olhe para o jogador
        target = stateMachine.trackingObject;
        if (target)
        {
            Vector3 tpos = target.transform.position;
            transform.rotation = mainBody.rotation;
            Vector3 targetPostition = new Vector3(tpos.x, transform.position.y, tpos.z);
            mainBody.LookAt(targetPostition);
        }

        // Jogue a bomba
        StartCoroutine(SimulateProjectile());

        // Reinicie nosso timer de espera
        duration = 0;
    }

    public override State RunCurrentState()
    {
        if (duration >= attackDuration)
        {
            if (setNewWaitTimeOnEnd)
                stateMachine.SetWaitingTime(newWaitTime);
            return followupState;
        }

        duration += Time.deltaTime;
        return this;
    }


    IEnumerator SimulateProjectile()
    {
        RaycastHit hit;
        Vector3 finalPos = target.transform.position;
        bool ray = Physics.Raycast(finalPos, Vector3.down, out hit, bombCollisionMask);
        if (ray)
            finalPos = hit.point;
        // Short delay added before Projectile is thrown
        yield return new WaitForSeconds(bombTossStartup);
        if (stateMachine.currentState != this)
            yield break;
        Vector3 pos = bombSpawnLocation.position != Vector3.zero ? bombSpawnLocation.position : mainBody.position;
        ShootableBomb newBomb = Instantiate(bomb, pos, Quaternion.identity);
        Transform Projectile = newBomb.transform;

        // Calculate distance to target
        float target_Distance = Vector3.Distance(Projectile.position, finalPos);

        // Calculate the velocity needed to throw the object to the target at specified angle.
        float projectile_Velocity = target_Distance / (Mathf.Sin(2 * firingAngle * Mathf.Deg2Rad) / gravity);

        // Extract the X  Y componenent of the velocity
        float Vx = Mathf.Sqrt(projectile_Velocity) * Mathf.Cos(firingAngle * Mathf.Deg2Rad);
        float Vy = Mathf.Sqrt(projectile_Velocity) * Mathf.Sin(firingAngle * Mathf.Deg2Rad);

        // Calculate flight time.
        float flightDuration = target_Distance / Vx;

        // Rotate projectile to face the target.
        Projectile.rotation = Quaternion.LookRotation(finalPos - Projectile.position);

        float elapse_time = 0;

        while (Projectile && elapse_time < flightDuration && !newBomb.exploded)
        {
            Projectile.Translate(0, (Vy - gravity * elapse_time) * Time.deltaTime, Vx * Time.deltaTime);
            elapse_time += Time.deltaTime;
            yield return null;
        }

        if (Projectile && !newBomb.exploded)
        {
            newBomb.Explode();
        }
    }
}
