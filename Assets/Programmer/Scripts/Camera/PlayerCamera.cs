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

    private RoomCamera roomCamera;//部屋のカメラ

    private GameObject roomCameraObject;//部屋のカメラ
    
    [HideInInspector] public Vector3 cameraForward = Vector3.zero;//カメラから見た方向
    [HideInInspector] public Vector3 cameraRight = Vector3.zero;//カメラの右方向ベクトル

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
        roomCameraObject.GetComponent<Camera>().enabled = true;

        roomCamera = roomCameraObject.GetComponent<RoomCamera>();
    }

    // Update is called once per frame
    void Update()
    {
        //現在のカメラの処理
        cameraRight = roomCameraObject.transform.right;
        cameraForward = roomCameraObject.transform.forward;

        //プレイヤーに追従して移動
        Vector3 moveAmount = playerData.currentRoomData.GetPlayerRoomData().transform.position - transform.position;

        moveAmount.y = 0.0f;

        //カメラの移動の制限値
        moveAmount.x = Mathf.Min(moveAmount.x, roomCamera.moveAmountLimit.x);
        moveAmount.x = Mathf.Max(moveAmount.x, -roomCamera.moveAmountLimit.x);

        moveAmount.z = Mathf.Min(moveAmount.z, roomCamera.moveAmountLimit.z);
        moveAmount.z = Mathf.Max(moveAmount.z, -roomCamera.moveAmountLimit.z);

        roomCameraObject.transform.position = roomCamera.initPos - moveAmount;

        // レイキャストによるオブジェクトの透過処理
        RayCastTransparent();
    }

    /// <summary>
    /// 部屋移動した際のカメラ情報更新
    /// </summary>
    public void OnRoomMove()
    {
        roomCameraObject.GetComponent<Camera>().enabled = false;

        GameObject currentRoom = playerData.currentRoomData.GetPlayerRoomData();
        roomCameraObject = currentRoom.transform.GetComponentInChildren<Camera>().gameObject;
        
        roomCamera = roomCameraObject.GetComponent<RoomCamera>();

        if (playerData.currentMode == PlayerData.PlayerMode.Normal)
            roomCameraObject.GetComponent<Camera>().enabled = true;
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
            r.SetPropertyBlock(mpb);

            currentHits.Add(r);
        }
    }
}
