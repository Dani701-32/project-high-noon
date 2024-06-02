using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerItem : MonoBehaviour
{
    [SerializeField, ReadOnly]
    private string playerId;

    [SerializeField, ReadOnly]
    private string playerName;

    [SerializeField]
    TMP_Text textPlayerName;

    public void SetPlayer(string playerId, string playerName)
    {
        this.playerId = playerId;
        this.playerName = playerName;
        SetUi();
    }

    private void SetUi()
    {
        textPlayerName.text = playerName;
    }
}
