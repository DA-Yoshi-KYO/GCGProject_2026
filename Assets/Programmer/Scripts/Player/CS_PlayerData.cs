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
        customInputAction = new CustomInputAction();
        customInputAction.Player.Enable();

        // プレイヤーの現在の部屋データの取得
        currentRoomData = GameObject.Find("RoomManager").GetComponent<CS_RoomPlayerPosition>();
    }

    public void ChangePlayerRoomData()
    {
        // プレイヤーの現在の部屋データを更新
        currentRoomData = GameObject.Find("RoomManager").GetComponent<CS_RoomPlayerPosition>();
        GameObject i = currentRoomData.GetPlayerRoomData();
        Debug.Log("[RoomMovePoint] PlayerData.currentRoomDataを更新しました。" + i.name);
    }

    private void OnDestroy()
    {
        // プレイヤーの入力アクションの無効化
        if (customInputAction != null)
            customInputAction.Player.Disable();
    }


}
