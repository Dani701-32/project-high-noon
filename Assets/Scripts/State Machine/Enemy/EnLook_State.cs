using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnLook_State : StateFSM
{
    public EnemyFSM enemy;

    public EnLook_State(EnemyFSM enemy)
    { this.enemy = enemy; }
    
    public void Enter()
    {
        enemy.StartCoroutine("ienum");
    }
    
    public void Update()
    {

    }

    public void Exit()
    {
        
    }
    
    public IEnumerator ienum()
    {
        Debug.Log("penis");
        yield return new WaitForSeconds(0.1f);
    }
}
