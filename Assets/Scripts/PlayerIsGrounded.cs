// Created by Matan Gazit
// PlayerIsGrounded.cs
// 
// Works in conjunction with player controller script
// This is a simple script for checking whether or not a player
//  can succesfully command a player controller prefab to jump

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIsGrounded : MonoBehaviour
{
    PlayerController PC;
    // Start is called before the first frame update

    void Awake()
    {
        PC = GetComponentInParent<PlayerController>();
    }

    void OnTriggerEnder(Collider other)
    {
        if(other.gameObject == PC.gameObject) return;
        PC.SetGrounded(true);
    }

    void OnTriggerExit(Collider other)
    {
        if(other.gameObject == PC.gameObject) return;
        PC.SetGrounded(false);
    }

    void OnTriggerStay(Collider other)
    {
        if(other.gameObject == PC.gameObject) return;
        PC.SetGrounded(true);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == PC.gameObject) return;
        PC.SetGrounded(true);
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject == PC.gameObject) return;
        PC.SetGrounded(false);
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject == PC.gameObject) return;
        PC.SetGrounded(true);
    }


}
