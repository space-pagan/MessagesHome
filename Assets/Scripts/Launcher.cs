// Created by Matan Gazit
// Launcher.cs
//
// Created with the help of rugbug redfern tutorials
// https://www.patreon.com/rugbug
//
// Multiplayer backend script which communicates with the Photon master client
//  This script connects to the master client on startup and then manages room
//  state and how the current user is associated with the rooms that currently
//  exist on the photon master client

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

// PunCallbacks gives access to callbacks for PUN methods
public class Launcher : MonoBehaviourPunCallbacks
{

    public static Launcher launcher;
    [SerializeField] TMP_InputField roomNameInput;
    [SerializeField] TMP_Text errorText;
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] Transform roomListContent;
    [SerializeField] GameObject roomListItem;
    [SerializeField] Transform playerListContent;
    [SerializeField] GameObject PlayerListItemPrefab; // Must be capitalized - Matan 10/31/2020
    [SerializeField] GameObject startButton;
    // Start is called before the first frame update
    void Awake()
    {
        launcher = this;
    }
    void Start()
    { 
        // Connects to master server
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Joined master...");
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnJoinedLobby()
    {
        MenuManager.MM.OpenMenu("Title");
        Debug.Log("Joined a lobby...");
        PhotonNetwork.NickName = "Player " + Random.Range(0, 1000).ToString("0000");
    }

    public void CreateRoom()
    {
        Debug.Log("Creating room...");
        if (string.IsNullOrEmpty(roomNameInput.text))
        {
            return;
        }
        PhotonNetwork.CreateRoom(roomNameInput.text);
        MenuManager.MM.OpenMenu("Loading");
    }

    public void JoinRoom(RoomInfo info)
    {
        if (info.PlayerCount < 2) {
            PhotonNetwork.JoinRoom(info.Name);
            MenuManager.MM.OpenMenu("Loading");
        } else {
            Debug.Log("Failed to join room...");
            errorText.text = "Failed to join room: Maximum players per room is 2";
            MenuManager.MM.OpenMenu("Error");
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room...");
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
        MenuManager.MM.OpenMenu("Room");

        Player[] players = PhotonNetwork.PlayerList;

        foreach(Transform child in playerListContent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < players.Length; i++)
        {
            Instantiate(PlayerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(players[i]);

        }

        startButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        startButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public void StartGame()
    {
        PhotonNetwork.LoadLevel(1);
    }

    public void CancelCreate()
    {
        Debug.Log("Creation cancelled...");
        MenuManager.MM.OpenMenu("Title");
    }

    public void HideOptions()
    {
        Debug.Log("Options menu hidden...");
        MenuManager.MM.OpenMenu("Title");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to create room...");
        errorText.text = "Failed to create room: " + message;
        MenuManager.MM.OpenMenu("Error");
    }

    public void LeaveRoom()
    {
        Debug.Log("Leaving room...");
        PhotonNetwork.LeaveRoom();
        MenuManager.MM.OpenMenu("Loading");
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Left Room...");
        MenuManager.MM.OpenMenu("Title");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("Room list has been updated...");
        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].RemovedFromList) {
                foreach (Transform trans in roomListContent) {
                    if (trans.gameObject.GetComponent<RoomListItem>().name.Equals(roomList[i].Name)) {
                        Destroy(trans.gameObject);
                    }
                }
            } else {
                Debug.Log("Room: " + roomList[i].Name);
                GameObject room = Instantiate(roomListItem, roomListContent);
                room.GetComponent<RoomListItem>().SetUp(roomList[i]);
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(PlayerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer);
    }
}
