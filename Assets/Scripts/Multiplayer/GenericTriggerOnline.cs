using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

public class GenericTriggerOnline : NetworkBehaviour
{
    [SerializeField] int amount = 30;
    [SerializeField] float waitTimeForEventStart;
    [SerializeField] bool vanishOnTrigger = true;
    [SerializeField] GameObject[] objects;
    [SerializeField] float respawnAfterSeconds;
    [SerializeField, ReadOnly] PlayerOnline player;

    bool running;
    Collider trigger;
    // Start is called before the first frame update
    void Start()
    {
        trigger = GetComponent<Collider>();
        waitTimeForEventStart = Mathf.Max(0, waitTimeForEventStart);
    }
    void End()
    {
        if (IsOwner)
        {
            End_ServerRpc();
        }
    }
    [ServerRpc]
    void End_ServerRpc()
    {
        trigger.enabled = false;
        foreach (GameObject obj in objects)
        {
            obj.SetActive(false);
        }
        if (respawnAfterSeconds > 0)
        {
            StartCoroutine(WaitRespawn());
        }
        End_ClientRpc();
    }
    [ClientRpc]
    void End_ClientRpc()
    {
        trigger.enabled = false;
        foreach (GameObject obj in objects)
        {
            obj.SetActive(false);
        }
        if (respawnAfterSeconds > 0)
        {
            StartCoroutine(WaitRespawn());
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (running) return;
        if (!other.CompareTag("Player")) return;
        player = other.GetComponent<PlayerOnline>();

        player.AddAmmo();
        if (vanishOnTrigger) End();
    }
    IEnumerator WaitRespawn()
    {
        yield return new WaitForSeconds(respawnAfterSeconds);
        trigger.enabled = true;
        foreach (GameObject obj in objects)
        {
            obj.SetActive(true);
        }
    }
}
