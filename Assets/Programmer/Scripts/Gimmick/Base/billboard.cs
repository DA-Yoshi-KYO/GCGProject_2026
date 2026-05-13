using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class billboard : MonoBehaviour
{
    public GameObject Player;
    private CS_PlayerCamera playerCamera;
    private CS_RoomCamera roomCamera;
    void Update()
    {
        if (Player == null)
        {
            Player = GameObject.FindGameObjectWithTag("Player");
        }
        else
        {
            playerCamera = Player.GetComponent<CS_PlayerCamera>();
            roomCamera = playerCamera.roomCamera;
            transform.rotation = Quaternion.LookRotation(transform.position - roomCamera.transform.position);
        }
    }
}
