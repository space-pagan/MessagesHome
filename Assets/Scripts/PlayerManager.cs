// Created by Matan Gazit
// PlayerManager.cs
//
// Player manager script keeps track of the player controller
// If a new user enters the world, this script will instantiate said user's controller

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class PlayerManager : MonoBehaviour
{
    // THIS WILL NOT BE IN THE FINAL GAME, I WILL HAVE AN ARRAY PULLING TRANSFORM POSITIONS BASED ON HOST/NOT HOST
    public int spawnX, spawnZ;
    Vector3 spawnVector; 
    PhotonView PV;

    void Awake()
    {
        MapGenerator gen = FindObjectOfType<MapGenerator>();
        //spawnVector = new Vector3(spawnX, gen.GetHeightAt(spawnX, spawnZ) + 2f, spawnZ);
        spawnVector = new Vector3(spawnX, 20f, spawnZ);
        PV = GetComponent<PhotonView>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (PV.IsMine)
        {
            Debug.Log("PV is mine");
            CreateController();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void CreateController()
    {
        Debug.Log("Is Master Client: " + PhotonNetwork.IsMasterClient);
        if (PhotonNetwork.IsMasterClient) {
            PV.Group = 0;
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerController"), spawnVector, Quaternion.identity, 0);
        } else {
            PV.Group = 1;
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "UI"), Vector3.zero, Quaternion.identity, 1);
        }
        Debug.Log("Group: " + PV.Group);
        Debug.Log("Instantiated player controller...");
    }
}
