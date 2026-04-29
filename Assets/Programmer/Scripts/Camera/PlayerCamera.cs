/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    プレイヤーカメラ作成
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-04-24 | 初回作成
 * 
 */
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    private PlayerData playerData;

    private GameObject roomCameraObject;//部屋のカメラ
    private GameObject upCameraObject;//上視点のカメラ

    [HideInInspector] public Vector3 cameraForward = Vector3.zero;//カメラから見た方向
    [HideInInspector] public Vector3 cameraRight = Vector3.zero;//カメラの右方向ベクトル

    [Header("上視点カメラの距離")][SerializeField] private float upDirection = 10.0f;

    private PlayerData.PlayerMode prevMode = PlayerData.PlayerMode.Normal;  // 切り替え感知用保存変数

    // Start is called before the first frame update
    void Start()
    {
        playerData = GetComponent<PlayerData>();
        GameObject currentRoom = playerData.currentRoomData.GetPlayerRoomData();

        roomCameraObject = currentRoom.transform.GetComponentInChildren<Camera>().gameObject;
        roomCameraObject.GetComponent<Camera>().depth = 1;
        
        upCameraObject = GameObject.Find("UpCamera");
        Vector3 upCameraPos = currentRoom.transform.position;
        upCameraPos.y += upDirection;
        upCameraObject.transform.position = upCameraPos;
        upCameraObject.GetComponent<Camera>().depth = -1;
    }

    // Update is called once per frame
    void Update()
    {
        PlayerData.PlayerMode currentMode = playerData.currentMode;

        if (currentMode != prevMode)
        {
            switch (currentMode)
            {
                case PlayerData.PlayerMode.Normal:
                    roomCameraObject.GetComponent<Camera>().depth = 1;
                    upCameraObject.GetComponent<Camera>().depth = -1;
                    break;
                case PlayerData.PlayerMode.Setting:
                    roomCameraObject.GetComponent<Camera>().depth = -1;
                    upCameraObject.GetComponent<Camera>().depth = 1;
                    break;
                default:
                    break;
            }
        }

        //プレイヤーのモードによってカメラ切り替え
        switch (currentMode)
        {
            case PlayerData.PlayerMode.Normal:
                RoomCamera();
                break;
            case PlayerData.PlayerMode.Setting:
                UpCamera();
                break;
            default:
                break;
        }

        prevMode = currentMode;

    }

    private void LateUpdate()
    {
        //roomCameraObject.transform.LookAt(gameObject.transform.position);
        
    }

    //部屋のカメラ処理
    private void RoomCamera()
    {
        cameraRight = roomCameraObject.transform.right;
        cameraForward = roomCameraObject.transform.forward;
    }

    //上視点のカメラ処理
    private void UpCamera()
    {
        cameraRight = upCameraObject.transform.right;
        cameraForward = upCameraObject.transform.up;
    }

    public void OnRoomMove()
    {
        roomCameraObject.GetComponent<Camera>().depth = -1;

        GameObject currentRoom = playerData.currentRoomData.GetPlayerRoomData();
        roomCameraObject = currentRoom.transform.GetComponentInChildren<Camera>().gameObject;
        
        if (playerData.currentMode == PlayerData.PlayerMode.Normal)
            roomCameraObject.GetComponent<Camera>().depth = 1;

        Vector3 upCameraPos = currentRoom.transform.GetChild(0).Find ("Center").transform.position;
        upCameraPos.y += upDirection;
        upCameraObject.transform.position = upCameraPos;
    }
}
