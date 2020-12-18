// Created by Matan Gazit
// PlayerListItem.cs
//
// Created with the help of Rugbug Redfern tutorials
// https://www.patreon.com/rugbug
//
// Script updates what players are currently in a created room

using Photon.Pun;
using TMPro;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerListItem : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_Text text;
    Player player;
    // Start is called before the first frame update
    public void SetUp(Player _player)
    {
        player = _player;
        text.text = _player.NickName;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if(player == otherPlayer)
        {
            Destroy(gameObject);
        }
    }

    public override void OnLeftRoom()
    {
        Destroy(gameObject);
    }
}
