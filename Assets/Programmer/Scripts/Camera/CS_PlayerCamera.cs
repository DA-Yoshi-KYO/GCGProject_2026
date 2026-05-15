/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    プレイヤーカメラ作成
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-04-24 | 初回作成
 * 2026-04-30 | レイキャストによる透過処理の作成(吉田)
 * 2026-05-11 | カメラの遷移演出の作成(元浪)
 * 2026-05-13 | リファクタリング（元浪）
 */
using System.Collections.Generic;
using UnityEngine;

public class CS_PlayerCamera : MonoBehaviour
{
    private PlayerData playerData;// プレイヤーのデータ

    [HideInInspector] public CS_RoomCamera roomCamera;//部屋のカメラ

    private GameObject roomCameraObject;//部屋のカメラ

    private GameObject currentRoom;//現在の部屋

    [Header("カメラの遷移の回転にかける時間")][SerializeField] private float rotateDuration = 1.0f;//回転にかける時間
    [Header("カメラの遷移の移動にかける時間")][SerializeField] private float moveDuration = 1.0f;//移動にかける時間
    [Header("カメラの追従にかける時間")][SerializeField] private float trackingTime = 0.5f;//追従にかける時間

    private float time = 0.0f;//時間

    struct TransitionCameraInfo
    {
        public Vector3 position;
        public Quaternion rotation;
    }
    private TransitionCameraInfo prevTransform;    // 前のカメラ位置
    private TransitionCameraInfo newTransform;     // 新しいカメラ位置

    //カメラの遷移演出の状態
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

    // Start is called before the first frame update
    void Start()
    {
        playerData = GetComponent<PlayerData>();

        // 現在の部屋を取得し、カメラの初期化を行う
        currentRoom = playerData.currentRoomData.GetPlayerRoomData();

        roomCameraObject = currentRoom.transform.GetComponentInChildren<Camera>().gameObject;
        roomCameraObject.GetComponent<Camera>().enabled = true;

        roomCamera = roomCameraObject.GetComponent<CS_RoomCamera>();
    }

    // Update is called once per frame
    void Update()
    {
        //現在のカメラの処理
        cameraRight = roomCameraObject.transform.right;
        cameraForward = roomCameraObject.transform.forward;

        //プレイヤーに追従して移動
        Vector3 moveAmount = currentRoom.transform.position - transform.position;

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

        //カメラの遷移演出の処理
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

    //==カメラの遷移の演出の処理==

    //他の更新の停止と現在の部屋のカメラ情報の保持
    private void StartTransitionCamera()
    {
        //更新を停止
        Time.timeScale = 0.0f;

        //現在のカメラ情報を保存
        prevTransform = new TransitionCameraInfo
        {
            position = roomCameraObject.transform.position,
            rotation = roomCameraObject.transform.rotation
        };

        //終値点
        Vector3 endPos = currentRoom.transform.position;
        endPos.y += 10.0f;
        Quaternion endRotate = Quaternion.Euler(90f, 180.0f, 0.0f);
        newTransform.position = endPos;
        newTransform.rotation = endRotate;

        transitionCamera = TransitionCamera.CameraIn;

        time = 0.0f;
    }

    //現在の部屋のカメラの回転移動処理
    private void TransitionCameraIn()
    {
        //移動処理
        time += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(time / rotateDuration);
        t = Easing.EaseInOutCubic(t);

        roomCameraObject.transform.position = Vector3.Lerp(prevTransform.position, newTransform.position, t);
        roomCameraObject.transform.rotation = Quaternion.Slerp(prevTransform.rotation, newTransform.rotation, t);
    
        //回転完了
        if(time > rotateDuration)
        {
            //現在のカメラ情報を保存
            prevTransform = newTransform;

            //終値点
            GameObject newRoom = playerData.currentRoomData.GetPlayerRoomData();
            Vector3 endPos = newRoom.transform.position;
            endPos.y += 10.0f;
            Quaternion endRotate = Quaternion.Euler(90f, 180.0f, 0.0f);
            newTransform.position = endPos;
            newTransform.rotation = endRotate;

            transitionCamera = TransitionCamera.CameraMove;

            time = 0.0f;
        }
    }

    //現在の部屋から次の部屋へのカメラの移動処理
    private void TransitionCameraMove()
    {
        time += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(time / moveDuration);
        t = Easing.EaseInOutCubic(t);

        roomCameraObject.transform.position = Vector3.Lerp(prevTransform.position, newTransform.position, t);
        roomCameraObject.transform.rotation = Quaternion.Slerp(prevTransform.rotation, newTransform.rotation, t);

        if (time > moveDuration)
        {
            //現在の部屋を更新
            GameObject newRoom = playerData.currentRoomData.GetPlayerRoomData();
            GameObject newCamera = newRoom.transform.GetComponentInChildren<Camera>().gameObject;

            //現在のカメラ情報を保存
            prevTransform = newTransform;

            //終値点
            newTransform = new TransitionCameraInfo
            {
                position = newCamera.transform.position,
                rotation = newCamera.transform.rotation
            };

            transitionCamera = TransitionCamera.CameraOut;

            time = 0.0f;
        }
    }

    //次の部屋のカメラの回転移動処理
    private void TransitionCameraOut()
    {
        //移動処理
        time += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(time / rotateDuration);
        t = Easing.EaseInOutCubic(t);

        roomCameraObject.transform.position = Vector3.Lerp(prevTransform.position, newTransform.position, t);
        roomCameraObject.transform.rotation = Quaternion.Slerp(prevTransform.rotation, newTransform.rotation, t);

        //回転完了
        if (time > rotateDuration)
        {
            time = 0.0f;

            transitionCamera = TransitionCamera.End;
        }
    }

    //他の更新を再開とカメラの次の部屋のカメラの切り替え処理
    private void EndTransitionCamera()
    {
        //更新を再開
        Time.timeScale = 1.0f;

        // 動かした前のカメラを無効にして、元の位置に戻す
        roomCamera = roomCameraObject.GetComponent<CS_RoomCamera>();
        roomCameraObject.GetComponent<Camera>().enabled = false;
        roomCameraObject.transform.position = roomCamera.initPos;
        roomCameraObject.transform.rotation = roomCamera.initRotate;

        // カメラを新しい部屋のカメラに切り替える
        currentRoom = playerData.currentRoomData.GetPlayerRoomData();
        roomCameraObject = currentRoom.transform.GetComponentInChildren<Camera>().gameObject;
        roomCamera = roomCameraObject.GetComponent<CS_RoomCamera>();
        roomCameraObject.GetComponent<Camera>().enabled = true;
       
        transitionCamera = TransitionCamera.None;   // カメラの遷移終了
    }
    //========================
}
