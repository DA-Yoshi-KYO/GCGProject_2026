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
            if (playerCamera == null) return;
            roomCamera = playerCamera.roomCamera;
            if (roomCamera == null) return;
            transform.rotation = Quaternion.LookRotation(transform.position - roomCamera.transform.position);
        }
    }
}
