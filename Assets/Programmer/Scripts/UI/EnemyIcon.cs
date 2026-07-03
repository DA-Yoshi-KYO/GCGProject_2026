using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyIcon : MonoBehaviour
{
    [Header("HPゲージのUI")]
    [SerializeField] private TextMeshProUGUI Hp;

    [Header("アイコンのImage")]
    [SerializeField] private Image iconImage;

    [Header("アイコンのサイズ")]
    [SerializeField] private Vector3 iconSize = new Vector3(1f, 1f, 1f);


    private CS_ThiefAI thiefAI;
    private CS_RoomPlayerPosition roomPlayerPosition;
    private void Start()
    {
        roomPlayerPosition = GameObject.Find("RoomManager").GetComponent<CS_RoomPlayerPosition>();

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
        if(Hp == null)
        {
            return;
        }
        int currentHP = thiefAI.read_Durability;
        Hp.text = currentHP.ToString();
        // 同じ部屋にいる場合サイズを大きくする
        if (thiefAI.read_MemorySystem.read_CurrentRoomPoint == roomPlayerPosition.PlayerRoomData.transform)
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
