using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherRoomsEnemy : MonoBehaviour
{
    [SerializeField]private NumberView numberView;
    [SerializeField] private string thiefTag = "Thief";

    private GameObject parentThief;
    private GameObject player;

    private int otherRoomsEnemyCount = 0;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Playerオブジェクトが見つかりません。");
            return;
        }
        numberView.SetTensView(false);
        parentThief = parentThief = GameObject.Find(thiefTag);
    }

    private void Update()
    {
        UpdateCount();
        numberView.SetNumber(otherRoomsEnemyCount);
    }

    private void UpdateCount()
    {
        if(parentThief == null)
        {
            Debug.LogError("Parent Thiefオブジェクトが見つかりません。");
            return;
        }
        if(player == null)
        {
            Debug.LogError("Playerオブジェクトが見つかりません。");
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
            if (index != playerRoomNumber) continue;
            count++;
        }
        otherRoomsEnemyCount = count;
    }
}
