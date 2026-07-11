using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyIcon : MonoBehaviour
{
    [Header("アイコンのImage")]
    [SerializeField] private Image iconImage;

    [Header("アイコンのサイズ")]
    [SerializeField] private Vector3 iconSize = new Vector3(1f, 1f, 1f);

    [SerializeField] private NumberView numberView;


    private CS_ThiefAI thiefAI;
    private CS_RoomPlayerPosition roomPlayerPosition;
    private void Start()
    {
        GameObject roomManager = GameObject.Find("RoomManager");
        if (roomManager != null)
        {
            roomPlayerPosition = roomManager.GetComponent<CS_RoomPlayerPosition>();
        }
        numberView.SetTensView(false);

    }
    void Update()
    {
        if (thiefAI == null)
        {
            return;
        }

        if (roomPlayerPosition == null)
        {
            return;
        }
 
        int currentHP = thiefAI.read_Durability;
        numberView.SetNumber(currentHP);
        // 同じ部屋にいる場合サイズを大きくする
        if (thiefAI.read_MemorySystem != null && roomPlayerPosition.PlayerRoomData != null &&
            thiefAI.read_MemorySystem.read_CurrentRoomPoint == roomPlayerPosition.PlayerRoomData.transform)
        {



            transform.localScale = Vector3.Lerp(transform.localScale, iconSize * 1.2f, Time.deltaTime * 5f);
        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, iconSize * 1.0f, Time.deltaTime * 5f);
        }

    }

    public void SetScript(CS_ThiefAI thiefAI)
    {
        this.thiefAI = thiefAI;
        iconImage.sprite = thiefAI.read_IconSprite;
    }


    public CS_ThiefAI GetScript() => thiefAI;
}
