/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    プレイヤーカメラ作成
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-04-24 | 初回作成
 * 
 */
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerCamera : MonoBehaviour
{
    private Camera roomCamera;//部屋のカメラ
    private Camera upCamera;//上視点のカメラ

    [HideInInspector] public Vector3 cameraForward = Vector3.zero;//カメラから見た方向
    [HideInInspector] public Vector3 cameraRight = Vector3.zero;//カメラの右方向ベクトル    

    [Header("カメラの位置を入り口からどれだけ高く離すか")][SerializeField] private float camHeight = 10.0f;

    // Start is called before the first frame update
    void Start()
    {
        Scene mainScene = SceneManager.GetSceneByName("MainScene");
        if (mainScene == null)
        {
            Debug.Log(mainScene.name + "が見つかりません");
            return;
        }

        // メインシーンからカメラの情報を受け取る
        roomCamera = GameObject.Find("RoomCamera").GetComponent<Camera>();
        upCamera = GameObject.Find("UpCamera").GetComponent<Camera>();

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

    private void LateUpdate()
    {
        Vector3 doorPos = GetComponent<CS_RoomPlayerPosition>().doorPoint.transform.position;
        Vector3 roomCamPos = roomCamera.gameObject.transform.position;
        roomCamPos = doorPos;
        roomCamPos.y = doorPos.y + camHeight;
        roomCamera.gameObject.transform.position = roomCamPos;
        roomCamera.gameObject.transform.LookAt(gameObject.transform.position);
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
