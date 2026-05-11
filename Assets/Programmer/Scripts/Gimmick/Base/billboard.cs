using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class billboard : MonoBehaviour
{
    public GameObject Player;
    private PlayerCamera playerCamera;
    private RoomCamera roomCamera;
    void Update()
    {
        if (Player == null)
        {
            Player = GameObject.FindGameObjectWithTag("Player");
        }
        else
        {
            playerCamera = Player.GetComponent<PlayerCamera>();
            roomCamera = playerCamera.roomCamera;
            transform.rotation = Quaternion.LookRotation(transform.position - roomCamera.transform.position);
        }
    }
}
