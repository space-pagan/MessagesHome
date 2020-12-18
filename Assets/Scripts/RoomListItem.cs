// Created by Matan Gazit
// RoomListItem.cs
//
// Script controls behavior the available rooms a user can join
// This list appears in the Join Room menu from the Title Menu

using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RoomListItem : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    public RoomInfo info;
    public string name;

    public void SetUp(RoomInfo _info)
    {
        info = _info;
        text.text = _info.Name;
        name = _info.Name;
    }

    public void OnClick()
    {
        Launcher.launcher.JoinRoom(info);
    }
}
