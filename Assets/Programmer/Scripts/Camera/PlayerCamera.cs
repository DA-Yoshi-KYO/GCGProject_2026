/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    プレイヤーカメラ作成
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-04-24 | 初回作成
 * 2026-04-30 | レイキャストによる透過処理の作成(吉田)
 * 2026-05-11 | カメラの遷移演出の作成(元浪)
 */
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    private PlayerData playerData;// プレイヤーのデータ

    private RoomCamera roomCamera;//部屋のカメラ

    private GameObject roomCameraObject;//部屋のカメラ

    private GameObject currentRoom;//現在の部屋

    [Header("カメラの遷移の回転にかける時間")][SerializeField] private float rotateDuration = 1.0f;//回転にかける時間
    [Header("カメラの遷移の移動にかける時間")][SerializeField] private float moveDuration = 1.0f;//移動にかける時間
    [Header("カメラの追従にかける時間")][SerializeField] float trackingTime = 0.5f;//追従にかける時間 
    private bool justOnce = false;//複数回実行しないように

    [HideInInspector] public Vector3 cameraForward = Vector3.zero;//カメラから見た方向
    [HideInInspector] public Vector3 cameraRight = Vector3.zero;//カメラの右方向ベクトル

    [Header("透過するオブジェクトのレイヤー")][SerializeField] LayerMask obstacleLayer;  // 透過するオブジェクトのレイヤー
    [Header("透過する範囲")][Range(1.0f, 10.0f)][SerializeField] float radius = 1.5f;     // 透過する範囲
    [Header("透過した後のα値")][Range(0.0f, 1.0f)][SerializeField] float maskAlpha = 0.5f;  // 透過した後のα値
    Dictionary<Renderer, MaterialPropertyBlock> mpbCache = new Dictionary<Renderer, MaterialPropertyBlock>();   // マテリアルのプロパティ
    List<Renderer> currentHits = new List<Renderer>();  // レイキャストの結果衝突したRenderオブジェクトのリスト

    private PlayerData.PlayerMode prevMode = PlayerData.PlayerMode.Normal;  // 切り替え感知用保存変数

    // Start is called before the first frame update
    void Start()
    {
        playerData = GetComponent<PlayerData>();

        // 現在の部屋を取得し、カメラの初期化を行う
        currentRoom = playerData.currentRoomData.GetPlayerRoomData();

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

        roomCameraObject.transform.position = 
            Vector3.Lerp(roomCameraObject.transform.position, roomCamera.initPos - moveAmount, trackingTime * Time.deltaTime);

        // レイキャストによるオブジェクトの透過処理
        RayCastTransparent();
    }

    /// <summary>
    /// 部屋移動した際のカメラ情報更新
    /// </summary>
    public IEnumerator OnRoomMove()
    {
        yield return StartCoroutine(TransitionCameraIn());

        yield return StartCoroutine(TransitionCameraMove());

        yield return StartCoroutine(TransitionCameraOut());
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

    //==カメラの遷移移動処理==
    private IEnumerator TransitionCameraIn()
    {
        if (!justOnce)
            justOnce = true;

        //移動前の情報
        Vector3 startPos = roomCameraObject.transform.position;
        Quaternion startRotate = roomCameraObject.transform.rotation;

        //移動後の情報
        Vector3 endPos = currentRoom.transform.position;
        endPos.y += 10.0f;
        Quaternion endRotate = Quaternion.Euler(90f, 180.0f, 0.0f);

        float time = 0.0f;

        while (time < rotateDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / rotateDuration);
            t = Easing.EaseInOutCubic(t);

            roomCameraObject.transform.position = Vector3.Lerp(startPos, endPos, t);
            roomCameraObject.transform.rotation = Quaternion.Slerp(startRotate, endRotate, t);

            yield return null;
        }

        justOnce = false;
    }

    private IEnumerator TransitionCameraOut()
    {
        if (!justOnce)
            justOnce = true;

        //移動前の情報
        Vector3 startPos = currentRoom.transform.position;
        startPos.y += 10.0f;
        Quaternion startRotate = Quaternion.Euler(90f, 180.0f, 0.0f);

        //移動後の情報
        Vector3 endPos = roomCamera.initPos;
        Quaternion endRotate = roomCamera.initRotate;
        
        float time = 0.0f;

        while (time < rotateDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / rotateDuration);
            t = Easing.EaseInOutCubic(t);

            roomCameraObject.transform.position = Vector3.Lerp(startPos, endPos, t);
            roomCameraObject.transform.rotation = Quaternion.Slerp(startRotate, endRotate, t);

            yield return null;
        }

        justOnce = false;
    }

    private IEnumerator TransitionCameraMove()
    {
        if (!justOnce)
            justOnce = true;

        //移動前の情報
        Vector3 startPos = currentRoom.transform.position;
        startPos.y += 10.0f;
        Quaternion startRotate = Quaternion.Euler(90f, 180.0f, 0.0f);

        //カメラ切り替え
        roomCameraObject.GetComponent<Camera>().enabled = false;

        currentRoom = playerData.currentRoomData.GetPlayerRoomData();
        roomCameraObject = currentRoom.transform.GetComponentInChildren<Camera>().gameObject;

        roomCamera = roomCameraObject.GetComponent<RoomCamera>();

        roomCameraObject.GetComponent<Camera>().enabled = true;

        //移動後の情報
        Vector3 endPos = currentRoom.transform.position;
        endPos.y += 10.0f;
        Quaternion endRotate = Quaternion.Euler(90f, 180.0f, 0.0f);

        float time = 0.0f;

        while (time < moveDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / moveDuration);
            t = Easing.EaseInOutCubic(t);

            roomCameraObject.transform.position = Vector3.Lerp(startPos, endPos, t);
            roomCameraObject.transform.rotation = Quaternion.Slerp(startRotate, endRotate, t);

            yield return null;
        }

        justOnce = false;
    }

    //========================
}
