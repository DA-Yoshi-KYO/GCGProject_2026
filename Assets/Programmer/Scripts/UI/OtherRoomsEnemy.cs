using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherRoomsEnemy : MonoBehaviour
{
    [SerializeField]private NumberView numberView;
    [SerializeField] private string thiefTag = "ThiefParent";

    private GameObject parentThief;
    private GameObject player;

    private int otherRoomsEnemyCount = 0;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (numberView == null)
        {
            numberView = GetComponentInChildren<NumberView>();
        }
        if (numberView == null)
        {
            return;
        }

        numberView.SetTensView(false);
        parentThief = GameObject.Find(thiefTag);
    }

    private void Update()
    {
        if (numberView == null) return;

        UpdateCount();
        numberView.SetNumber(otherRoomsEnemyCount);
    }

    private void UpdateCount()
    {
        if(parentThief == null)
        {
            parentThief = GameObject.Find(thiefTag);
            Debug.Log("parentThief is null, trying to find it again.");
            return;
        }
        if(player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            Debug.Log("player is null, trying to find it again.");
            return;
        }
        int playerRoomNumber = CS_RoomCreatePointRaycast.GetRoomIndex(player);

        List<GameObject> thieves = new List<GameObject>();

        foreach (Transform child in parentThief.transform)
        {
            thieves.Add(child.gameObject);
        }

        int count = 0;

        foreach (GameObject thief in thieves)
        {
            int index = CS_RoomCreatePointRaycast.GetRoomIndex(thief);
            if (index == playerRoomNumber) continue;
            count++;
        }
        otherRoomsEnemyCount = count;
    }
}
