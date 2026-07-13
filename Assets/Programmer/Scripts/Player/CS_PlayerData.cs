using UnityEngine;

public class CS_PlayerData : MonoBehaviour
{
    [HideInInspector] public CustomInputAction customInputAction { private set; get; }  // プレイヤーの入力アクション
    [HideInInspector] public CS_RoomPlayerPosition currentRoomData { private set; get; }// プレイヤーの現在の部屋データ

    // プレイヤーのモード
    public enum PlayerMode
    {
        Normal,     // 通常
        Setting,    // 設置フェーズ
    };
    [HideInInspector] public PlayerMode currentMode = PlayerMode.Normal;

    void Awake()
    {
        // プレイヤーの入力アクションの初期化と有効化
        customInputAction = CS_CustomInputActionManager.instance.customInputAction;

        // プレイヤーの現在の部屋データの取得
        GameObject roomManager = GameObject.Find("RoomManager");
        if (roomManager != null)
        {
            currentRoomData = roomManager.GetComponent<CS_RoomPlayerPosition>();
        }
        else
        {
            Debug.LogError("RoomManagerが見つかりません。");
        }
    }

    public void ChangePlayerRoomData()
    {
        // プレイヤーの現在の部屋データを更新
        GameObject roomManager = GameObject.Find("RoomManager");
        if (roomManager == null)
        {
            Debug.LogError("RoomManagerが見つかりません。");
            return;
        }
        currentRoomData = roomManager.GetComponent<CS_RoomPlayerPosition>();
        if (currentRoomData == null) return;
        GameObject i = currentRoomData.GetPlayerRoomData();
        if (i == null) return;
        Debug.Log("[RoomMovePoint] PlayerData.currentRoomDataを更新しました。" + i.name);
    }
}
