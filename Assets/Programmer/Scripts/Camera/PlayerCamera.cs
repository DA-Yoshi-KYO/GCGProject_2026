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
using Unity.VisualScripting;
using UnityEditor.Rendering;
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

    private float time = 0.0f;
    private Transform prevCameraInfomation;
    private enum TransitionCamera
    {
        None,
        Start,
        CameraIn,
        CameraMove,
        CameraOut,
        End
    }

    private TransitionCamera transitionCamera = TransitionCamera.None;

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

        switch (transitionCamera)
        {
            case TransitionCamera.None:
                break;
            case TransitionCamera.Start:
                StartTransitionCamera();
                break;
            case TransitionCamera.CameraIn:
                TransitionCameraIn();
                break;
            case TransitionCamera.CameraMove:
                TransitionCameraMove();
                break;
            case TransitionCamera.CameraOut:
                TransitionCameraOut();
                break;
            case TransitionCamera.End:
                EndTransitionCamera();
                break;
        }
    }

    /// <summary>
    /// 部屋移動した際のカメラ情報更新
    /// </summary>
    public void OnRoomMove()
    {
        transitionCamera = TransitionCamera.Start;
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
    private void StartTransitionCamera()
    {
        //更新を停止
        Time.timeScale = 0.0f;

        //現在のカメラ情報を保存
        prevCameraInfomation = roomCameraObject.transform;

        transitionCamera = TransitionCamera.CameraIn;

        time = 0.0f;
    }

    private void TransitionCameraIn()
    {
        //終値点
        Vector3 endPos = currentRoom.transform.position;
        endPos.y += 10.0f;
        Quaternion endRotate = Quaternion.Euler(90f, 180.0f, 0.0f);

        //移動処理
        time += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(time / rotateDuration);
        t = Easing.EaseInOutCubic(t);

        roomCameraObject.transform.position = Vector3.Lerp(prevCameraInfomation.position, endPos, t);
        roomCameraObject.transform.rotation = Quaternion.Slerp(prevCameraInfomation.rotation, endRotate, t);
    
        //回転完了
        if(time > rotateDuration)
        {
            //カメラ切り替え
            roomCameraObject.GetComponent<Camera>().enabled = false;

            currentRoom = playerData.currentRoomData.GetPlayerRoomData();
            roomCameraObject = currentRoom.transform.GetComponentInChildren<Camera>().gameObject;

            roomCamera = roomCameraObject.GetComponent<RoomCamera>();

            roomCameraObject.GetComponent<Camera>().enabled = true;

            time = 0.0f;

            transitionCamera = TransitionCamera.CameraMove;
        }
    }

    private void TransitionCameraOut()
    {
        Vector3 startPos = currentRoom.transform.position;
        startPos.y += 10.0f;
        Quaternion startRotate = Quaternion.Euler(90f, 180.0f, 0.0f);

        time += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(time / rotateDuration);
        t = Easing.EaseInOutCubic(t);

        roomCameraObject.transform.position = Vector3.Lerp(startPos, roomCamera.initPos, t);
        roomCameraObject.transform.rotation = Quaternion.Slerp(startRotate, roomCamera.initRotate, t);

        if(time > rotateDuration)
        {
            time = 0.0f;

            transitionCamera = TransitionCamera.End;
        }
    }

    private void TransitionCameraMove()
    {
        Vector3 endPos = currentRoom.transform.position;
        endPos.y += 10.0f;
        Quaternion Rotate = Quaternion.Euler(90f, 180.0f, 0.0f);

        time += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(time / moveDuration);
        t = Easing.EaseInOutCubic(t);

        roomCameraObject.transform.position = Vector3.Lerp(prevCameraInfomation.position, endPos, t);
        roomCameraObject.transform.rotation = Quaternion.Slerp(Rotate, Rotate, t);

        if (time > moveDuration)
        {
            time = 0.0f;

            transitionCamera = TransitionCamera.CameraOut;
        }
    }
    private void EndTransitionCamera()
    {
        //更新を停止
        Time.timeScale = 1.0f;

        transitionCamera = TransitionCamera.None;
    }
    //========================
}
