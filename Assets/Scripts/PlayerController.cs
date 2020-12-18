// Created by Matan Gazit
// PlayerController.cs
//
// Created with the help of rugbug redfern tutorials
// https://www.patreon.com/rugbug
//
// Script controls all playter functionallity using Photon and Unity Generic library commands

using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject cameraHolder;
    [SerializeField] float mouseSens, wlkSpd, jmpForce, smoothTime;
    [SerializeField] Item[] items;
    public PlayerController PC;

    int itemIndex;
    int prevItemIndex = -1; //default value

    float verticalLookRotation;
    bool isGrounded;
    bool camDestroyed = false;
    Vector3 smoothMoveVelocity;
    Vector3 moveAmount;

    Rigidbody rb;

    PhotonView PV;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
    }

    // Not going to lie, I spelled Start as "Sart" and didn't notice for days...
    // ... And I got upset when it wouldn't work, before noticing the error...
    void Start()
    {
        //IsMine or AmOwner works here...

        // If this is the local player, equip first item in the inventory
        if (PV.AmOwner)
        {
            EquipItem(0);
        }
        else
        {
            // A client should only have one Camera and one rigidbody
            // The errors you get from multiple cameras are horrific
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(rb);
        }
    }

    void Update()
    {
        // AmOwner for component interaction, IsMine does not function
        // If this is not the local player, ignore input
        if (!PV.IsMine)
        {
            return;
        }
        
        //Debug.Log("PhotonView is mine");
        if (!GetComponentInChildren<OptionsMenu>().pauseActive) {
            Look();

            Move();

            Jump();

            for (int i = 0; i < items.Length; i++)
            {
                if (Input.GetKeyDown((i + 1).ToString()))
                {
                    EquipItem(i);
                    break;
                }
            }

            //Scroll wheel equipment functionality
            if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
            {
                if (itemIndex >= items.Length - 1)
                {
                    Debug.Log("Index looped +");
                    EquipItem(0);
                }
                else
                {
                    EquipItem(itemIndex + 1);
                }
            }
            else if(Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
            {
                if (itemIndex <= 0)
                {
                    Debug.Log("Index looped -");
                    EquipItem(items.Length - 1);
                }
                else
                {
                    EquipItem(itemIndex - 1);
                }
            }
        }
    }

    // Not called every interval for the sake of physics
    void FixedUpdate()
    {
        if (!PV.IsMine)
            return;

        rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
        if (rb.transform.position.y < -50) {
            rb.transform.position = new Vector3(0, 20, 0);
        }
    }

    // Movement functions, I think they're pretty straightforward
    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(transform.up * jmpForce);
        }
    }

    void Move()
    {
        Vector3 moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        moveAmount = Vector3.SmoothDamp(moveAmount, moveDirection * wlkSpd, ref smoothMoveVelocity, smoothTime);
    }

    void Look()
    {
        transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSens);

        verticalLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSens;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);

        cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
    }

    // Equipment switcher
    void EquipItem(int _index)
    {
        if (_index == prevItemIndex) return;

        itemIndex = _index;

        items[itemIndex].itemGameObject.SetActive(true);

        if (prevItemIndex != -1)
        {
            items[prevItemIndex].itemGameObject.SetActive(false);
        }

        prevItemIndex = itemIndex;

        //checks for local player
        if (PV.IsMine)
        {
            //adjust props and sent to master client
            Hashtable hash = new Hashtable();
            hash.Add("itemIndex", itemIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        // verify
        if (!PV.IsMine && targetPlayer == PV.Owner)
        {
            // do this on other clients
            EquipItem((int)changedProps["itemIndex"]);
        }
    }

    public void SetGrounded(bool _isGrounded)
    {
        isGrounded = _isGrounded;
    }
}
