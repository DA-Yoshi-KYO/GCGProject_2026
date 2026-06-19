using System.Collections.Generic;
using UnityEngine;

public class MiniMap : MonoBehaviour
{
    [SerializeField] private GameObject[] targetObjects;

    [SerializeField] private GameObject playerIcon;
    [SerializeField] private GameObject thiefIcon;
    [SerializeField] private GameObject treasureIcon;
    [SerializeField] private GameObject gimmickIcon;

    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string thiefTag = "Thief";

    private GameObject createRoomPints;

    private GameObject player;
    private GameObject parentThief;

    struct MiniMapObjectInfo
    {
        public GameObject playerIcon;
        public GameObject thiefIcon;
        public GameObject treasureIcon;
        public GameObject gimmickIcon;
    }

    struct MiniMapInfo
    {
        public bool isPlayerIconActive;
        public bool isThiefIconActive;
        public bool isTreasureIconActive;
        public bool isGimmickIconActive;
    }
    

    private List<MiniMapObjectInfo> miniMapObjectInfo;
    private List<MiniMapInfo> miniMapInfo;

    private void Start()
    {
        player = GameObject.Find(playerTag);
        parentThief = GameObject.Find(thiefTag);
        createRoomPints = GameObject.Find("RoomCreatePoints");

        miniMapObjectInfo = new List<MiniMapObjectInfo>(targetObjects.Length);
        miniMapInfo = new List<MiniMapInfo>(targetObjects.Length);

        // 各ターゲットオブジェクトに対してアイコンを生成し、MiniMapObjectInfoに格納
        for (int i = 0; i < targetObjects.Length; i++)
        {
            MiniMapObjectInfo info = new MiniMapObjectInfo();

            info.playerIcon = Instantiate(playerIcon, targetObjects[i].transform);
            info.playerIcon.transform.localScale = Vector3.one * 0.5f; 
            info.playerIcon.SetActive(false);

            info.thiefIcon  = Instantiate(thiefIcon, targetObjects[i].transform);
            info.thiefIcon.transform.localScale = Vector3.one * 0.5f;
            info.thiefIcon.SetActive(false);

            info.treasureIcon = Instantiate(treasureIcon, targetObjects[i].transform);
            info.treasureIcon.transform.localScale = Vector3.one * 0.5f;
            info.treasureIcon.SetActive(false);

            info.gimmickIcon = Instantiate(gimmickIcon, targetObjects[i].transform);
            info.gimmickIcon.transform.localScale = Vector3.one * 0.5f;
            info.gimmickIcon.SetActive(false);
            miniMapObjectInfo.Add(info);
        }
        // 各ターゲットオブジェクトに対してMiniMapInfoを初期化
        for (int i = 0; i < targetObjects.Length; i++)
        {
            MiniMapInfo info = new MiniMapInfo();
            info.isPlayerIconActive     = false;
            info.isThiefIconActive      = false;
            info.isTreasureIconActive   = false;
            info.isGimmickIconActive    = false;
            miniMapInfo.Add(info);
        }
    }

    private void Update()
    { 
        if(player == null)
        {
            player = GameObject.Find(playerTag);
            Debug.LogError("Player found");
            return;
        }
        if(parentThief == null)
        {
            parentThief = GameObject.Find(thiefTag);
            Debug.LogError("Thief found");
            return;
        }

        // フラグのリセット
        for(int i = 0; i < miniMapInfo.Count; i++)
            {
                MiniMapInfo info = miniMapInfo[i];
                info.isPlayerIconActive = false;
                info.isThiefIconActive = false;
                info.isTreasureIconActive = false;
                info.isGimmickIconActive = false;
                miniMapInfo[i] = info;
        }


        UpdatePlayerActive();
        UpdateThiefActive();
        UpdateTreasureActive();
        UpdateGimmickActive();

        RenderSetting();
    }

    private void UpdatePlayerActive()
    {
        int index = CS_RoomCreatePointRaycast.GetRoomIndex(player);
        index -= 1;
        MiniMapInfo info = miniMapInfo[index];
        info.isPlayerIconActive = true;
        miniMapInfo[index] = info;
    }

    private void UpdateThiefActive()
    {
        List<GameObject> thieves = new List<GameObject>();

        foreach (Transform child in parentThief.transform)
        {
            thieves.Add(child.gameObject);
        }

        foreach (GameObject thief in thieves)
        {
            int index = CS_RoomCreatePointRaycast.GetRoomIndex(thief);
            index -= 1;
            MiniMapInfo info = miniMapInfo[index];
            info.isThiefIconActive = true;
            miniMapInfo[index] = info;
        }

    }

    private void UpdateTreasureActive()
    {
    }

    private void UpdateGimmickActive()
    {
    }

    private void RenderSetting()
    {
        for (int i = 0 ; i < targetObjects.Length ; i++)
        {
            miniMapObjectInfo[i].playerIcon.SetActive(miniMapInfo[i].isPlayerIconActive);
            miniMapObjectInfo[i].thiefIcon.SetActive(miniMapInfo[i].isThiefIconActive);
            miniMapObjectInfo[i].treasureIcon.SetActive(miniMapInfo[i].isTreasureIconActive);
            miniMapObjectInfo[i].gimmickIcon.SetActive(miniMapInfo[i].isGimmickIconActive);

            // アクティブなアイコンを優先順位(プレイヤー→泥棒→財宝→ギミック)で集める
            List<RectTransform> activeIcons = new List<RectTransform>(4);

            if (miniMapInfo[i].isPlayerIconActive)
                activeIcons.Add(miniMapObjectInfo[i].playerIcon.GetComponent<RectTransform>());
            if (miniMapInfo[i].isThiefIconActive)
                activeIcons.Add(miniMapObjectInfo[i].thiefIcon.GetComponent<RectTransform>());
            if (miniMapInfo[i].isTreasureIconActive)
                activeIcons.Add(miniMapObjectInfo[i].treasureIcon.GetComponent<RectTransform>());
            if (miniMapInfo[i].isGimmickIconActive)
                activeIcons.Add(miniMapObjectInfo[i].gimmickIcon.GetComponent<RectTransform>());

            int count = activeIcons.Count;
            if (count == 0) continue;

            RectTransform parentRect = targetObjects[i].GetComponent<RectTransform>();
            Vector2[] positions = GetLayoutPositions(count, parentRect.rect.width, parentRect.rect.height);

            for (int j = 0 ; j < count ; j++)
            {
                activeIcons[j].anchoredPosition = positions[j];
            }
        }
    }

    private Vector2[] GetLayoutPositions(int count, float width, float height)
    {
        // 2x2グリッドの各セル中心座標(左上→右上→左下→右下の順)
        float quarterW = width * 0.25f;
        float quarterH = height * 0.25f;

        Vector2 topLeft = new Vector2(-quarterW, quarterH);
        Vector2 topRight = new Vector2(quarterW, quarterH);
        Vector2 bottomLeft = new Vector2(-quarterW, -quarterH);
        Vector2 bottomRight = new Vector2(quarterW, -quarterH);

        switch (count)
        {
            case 1:
                return new Vector2[] { Vector2.zero };

            case 2:
                return new Vector2[] { topLeft, topRight };

            case 3:
                return new Vector2[] { topLeft, topRight, bottomLeft };

            case 4:
                return new Vector2[] { topLeft, topRight, bottomLeft, bottomRight };

            default:
                return new Vector2[0];
        }
    }
}



