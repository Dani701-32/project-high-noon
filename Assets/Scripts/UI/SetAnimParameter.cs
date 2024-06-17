using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetAnimParameter : MonoBehaviour
{
    [SerializeField] Animator anim;
    
    public void ToggleBool(string boolean)
    {
        anim.SetBool(boolean, !anim.GetBool(boolean));
    }
    
}
