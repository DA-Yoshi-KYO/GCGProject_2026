/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    プレイヤーカメラ作成
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-04-24 | 初回作成
 * 2026-04-30 | レイキャストによる透過処理の作成(ヨシダ)
 */
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    private PlayerData playerData;// プレイヤーのデータ

    private GameObject roomCameraObject;//部屋のカメラ
    private GameObject upCameraObject;//上視点のカメラ

    [HideInInspector] public Vector3 cameraForward = Vector3.zero;//カメラから見た方向
    [HideInInspector] public Vector3 cameraRight = Vector3.zero;//カメラの右方向ベクトル

    [Header("上視点カメラの距離")][SerializeField] private float upDirection = 10.0f;    // 上視点カメラの距離
    [Header("透過するオブジェクトのレイヤー")][SerializeField] LayerMask obstacleLayer;  // 透過するオブジェクトのレイヤー
    [Header("透過する範囲")][Range(1.0f,10.0f)][SerializeField] float radius = 1.5f;     // 透過する範囲
    [Header("透過した後のα値")][Range(0.0f,1.0f)][SerializeField] float maskAlpha = 0.5f;  // 透過した後のα値
    Dictionary<Renderer, MaterialPropertyBlock> mpbCache = new Dictionary<Renderer, MaterialPropertyBlock>();   // マテリアルのプロパティ
    List<Renderer> currentHits = new List<Renderer>();  // レイキャストの結果衝突したRenderオブジェクトのリスト

    private PlayerData.PlayerMode prevMode = PlayerData.PlayerMode.Normal;  // 切り替え感知用保存変数

    // Start is called before the first frame update
    void Start()
    {
        playerData = GetComponent<PlayerData>();

        // 現在の部屋を取得し、カメラの初期化を行う
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
        //プレイヤーのモードによってカメラ切り替え
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

        // レイキャストによるオブジェクトの透過処理
        RayCastTransparent();
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

    /// <summary>
    /// 部屋移動した際のカメラ情報更新
    /// </summary>
    public void OnRoomMove()
    {
        roomCameraObject.GetComponent<Camera>().depth = -1;

        GameObject currentRoom = playerData.currentRoomData.GetPlayerRoomData();
        roomCameraObject = currentRoom.transform.GetComponentInChildren<Camera>().gameObject;

        if (playerData.currentMode == PlayerData.PlayerMode.Normal)
            roomCameraObject.GetComponent<Camera>().depth = 1;

        Vector3 upCameraPos = currentRoom.transform.GetChild(0).Find("Center").transform.position;
        upCameraPos.y += upDirection;
        upCameraObject.transform.position = upCameraPos;
    }

    private void RayCastTransparent()
    {
        // 前フレームのリセット
        foreach (var r in currentHits)
        {
            if (mpbCache.TryGetValue(r, out var mpb))
            {
                mpb.SetFloat("_EnableClip", 0);
                r.SetPropertyBlock(mpb);
            }
        }
        currentHits.Clear();

        // カメラとプレイヤーの距離の間でレイを制限してキャストを行う
        Vector3 camPos = roomCameraObject.transform.position;
        Vector3 playerPos = gameObject.transform.position;
        float playerDist = Vector3.Distance(playerPos, camPos);
        Ray ray = new Ray(camPos, (playerPos - camPos).normalized);
        RaycastHit[] hits = Physics.RaycastAll(ray, playerDist, obstacleLayer);

        foreach (var hit in hits)
        {
            // 衝突したRendererオブジェクトの取得
            Renderer r = hit.collider.GetComponent<Renderer>();
            if (r == null) continue;

            // マテリアルの取得
            if (!mpbCache.TryGetValue(r, out var mpb))
            {
                mpb = new MaterialPropertyBlock();
                mpbCache[r] = mpb;
            }

            // マテリアルのパラメータを設定
            mpb.SetFloat("_EnableClip", 1);
            Vector3 maskPos = playerPos;
            const float adjustY = 1.0f;
            maskPos.y += adjustY;
            mpb.SetVector("_PlayerPos", maskPos);
            mpb.SetFloat("_Radius", radius);
            mpb.SetFloat("_MaskAlpha", maskAlpha);
            r.SetPropertyBlock(mpb);

            currentHits.Add(r);
        }
    }
}
