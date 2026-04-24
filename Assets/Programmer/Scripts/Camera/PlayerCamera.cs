using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private Camera roomCamera;//部屋のカメラ
    [SerializeField] private Camera upCamera;//上視点のカメラ

    public Vector3 cameraForward = Vector3.zero;//カメラから見た方向
    public Vector3 cameraRight = Vector3.zero;//カメラの右方向ベクトル    

    // Start is called before the first frame update
    void Start()
    {
        roomCamera.depth = 1;
        upCamera.depth = -1;
    }

    // Update is called once per frame
    void Update()
    {
        //プレイヤーのモードによってカメラ切り替え
        switch (GetComponent<PlayerAction>().currentMode)
        {
            case PlayerAction.PlayerMode.Normal:
                roomCamera.depth = 1;
                upCamera.depth = -1;
                RoomCamera();
                break;
            case PlayerAction.PlayerMode.Setting:
                roomCamera.depth = -1;
                upCamera.depth = 1;
                UpCamera();
                break;
            default:
                break;
        }
    }

    //部屋のカメラ処理
    private void RoomCamera()
    {
        cameraRight = roomCamera.transform.right;
        cameraForward = roomCamera.transform.forward;
    }

    //上視点のカメラ処理
    private void UpCamera()
    {
        cameraRight = upCamera.transform.right;
        cameraForward = upCamera.transform.up;
    }

}
