using UnityEngine;

public class PlayerData : MonoBehaviour
{
    [HideInInspector] public CustomInputAction customInputAction { private set; get; }
    [HideInInspector] public CS_RoomPlayerPosition currentRoomData { private set; get; }

    //プレイヤーのモード
    public enum PlayerMode
    {
        Normal,     //通常
        Setting,    //設置フェーズ
    };
    [HideInInspector] public PlayerMode currentMode = PlayerMode.Normal;

    void Awake()
    {
        customInputAction = new CustomInputAction();
        customInputAction.Player.Enable();
        currentRoomData = GameObject.Find("RoomManager").GetComponent<CS_RoomPlayerPosition>();
    }

    private void OnDestroy()
    {
        customInputAction.Player.Disable();
    }
}
