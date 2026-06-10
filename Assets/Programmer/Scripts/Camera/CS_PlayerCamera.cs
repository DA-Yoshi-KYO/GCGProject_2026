/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    プレイヤーカメラ作成
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-04-24 | 初回作成
 * 2026-04-30 | レイキャストによる透過処理の作成(吉田)
 * 2026-05-11 | カメラの遷移演出の作成(元浪)
 * 2026-05-13 | リファクタリング（元浪）
 * 2026-05-27 | リファクタリング（吉田）
 * 2026-06-05 | 初期値のカメラの修正
 */
using System.Collections.Generic;
using UnityEngine;

public class CS_PlayerCamera : MonoBehaviour
{
    private CS_PlayerData playerData;// プレイヤーのデータ

    /* ---カメラの情報を更新する為の部屋情報--- */
    private GameObject currentRoom;// 現在の部屋
    Vector3 roomCenter = Vector3.zero;// 部屋の中心座標
    [HideInInspector] public CS_RoomCamera roomCamera;// 部屋のカメラ
    private GameObject roomCameraObject;// 部屋のカメラ

    /* ---カメラの遷移演出のための変数--- */
    [Header("カメラの遷移の回転にかける時間")][SerializeField] private float rotateDuration = 1.0f;//回転にかける時間
    [Header("カメラの遷移の移動にかける時間")][SerializeField] private float moveDuration = 1.0f;//移動にかける時間
    [Header("カメラの追従にかける時間")][SerializeField] private float trackingTime = 0.5f;//追従にかける時間

    private float transTimer = 0.0f;  // 時間
    
    struct TransitionCameraInfo     // カメラの遷移演出のための情報をまとめた構造体
    {
        public Vector3 position;    // カメラの位置
        public Quaternion rotation; // カメラの回転
    }
    private TransitionCameraInfo prevInfo;    // 前のカメラ情報
    private TransitionCameraInfo newInfo;     // 新しいカメラ情報
   
    private enum TransitionCamera    // カメラの遷移演出の状態
    {
        None,       // 遷移なし
        Start,      // 遷移開始
        CameraIn,   // カメラの回転移動
        CameraMove, // カメラの移動
        CameraOut,  // カメラの回転移動
        End         // 遷移終了
    }
    private TransitionCamera transitionCamera = TransitionCamera.None;

    /* ---移動に使用するためのベクトル--- */
    [HideInInspector] public Vector3 cameraForward { private set; get; } = Vector3.zero;    //カメラの正面方向ベクトル
    [HideInInspector] public Vector3 cameraRight { private set; get; } = Vector3.zero;      //カメラの右方向ベクトル

    /* ---レイキャストによるオブジェクトの透過処理のための変数--- */
    [Header("透過するオブジェクトのレイヤー")][SerializeField] LayerMask obstacleLayer;  // 透過するオブジェクトのレイヤー
    Dictionary<Renderer, MaterialPropertyBlock> mpbCache = new Dictionary<Renderer, MaterialPropertyBlock>();   // マテリアルのプロパティ
    List<Renderer> currentHits = new List<Renderer>();  // レイキャストの結果衝突したRenderオブジェクトのリスト

    // Start is called before the first frame update
    void Start()
    {
        playerData = GetComponent<CS_PlayerData>();
        if (playerData == null) Debug.LogError("CS_PlayerDataコンポーネントが見つかりませんでした。");

        // 現在の部屋を取得し、カメラの初期化を行う
        currentRoom = playerData.currentRoomData.GetPlayerRoomData();
        if (currentRoom == null) Debug.LogError("現在の部屋を取得できませんでした。");

        Transform roomCenterTransform = currentRoom.transform.GetChild(0).Find("Center");
        if (roomCenterTransform == null) roomCenter = currentRoom.transform.position;
        else roomCenter = roomCenterTransform.position;

        roomCameraObject = currentRoom.transform.GetComponentInChildren<Camera>().gameObject;
        if (roomCameraObject == null) Debug.LogError(currentRoom.name + "の部屋にカメラがありません。");
        else roomCameraObject.GetComponent<Camera>().enabled = true; // 初期部屋のカメラを有効にする
        
        roomCamera = roomCameraObject.GetComponent<CS_RoomCamera>();
        if (roomCamera == null) Debug.LogError(roomCameraObject.name + "にCS_RoomCameraコンポーネントがありません。");

        //視点がプレイヤーから始まるように補正
        Vector3 moveAmount = roomCenter - transform.position;
        moveAmount.y = 0.0f;
        
        //カメラの移動に制限をかける
        moveAmount.x = Mathf.Clamp(moveAmount.x, -roomCamera.moveAmountLimit.x, roomCamera.moveAmountLimit.x);
        moveAmount.z = Mathf.Clamp(moveAmount.z, -roomCamera.moveAmountLimit.z, roomCamera.moveAmountLimit.z);

        roomCameraObject.transform.position = roomCamera.initPos - moveAmount;
    }

    // Update is called once per frame
    void Update()
    {
        //現在のカメラのベクトルを更新
        cameraRight = roomCameraObject.transform.right;
        cameraForward = roomCameraObject.transform.forward;

        //プレイヤーに追従して移動
        Vector3 moveAmount = roomCenter - transform.position;

        moveAmount.y = 0.0f;

        //カメラの移動に制限をかける
        moveAmount.x = Mathf.Clamp(moveAmount.x, -roomCamera.moveAmountLimit.x, roomCamera.moveAmountLimit.x);
        moveAmount.z = Mathf.Clamp(moveAmount.z, -roomCamera.moveAmountLimit.z, roomCamera.moveAmountLimit.z);

        //カメラの移動
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

    /// <summary>
    /// カメラのレイとプレイヤーの間にあるオブジェクトを透過させる処理
    /// </summary>
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
        prevInfo = new TransitionCameraInfo
        {
            position = roomCameraObject.transform.position,
            rotation = roomCameraObject.transform.rotation
        };

        //終値点のカメラ情報を保存
        Vector3 endPos = currentRoom.transform.position;
        endPos.y += 10.0f;
        Quaternion endRotate = Quaternion.Euler(90f, 180.0f, 0.0f);
        newInfo.position = endPos;
        newInfo.rotation = endRotate;

        // カメラ遷移の状態を進める
        transitionCamera = TransitionCamera.CameraIn;

        // 時間のリセット
        transTimer = 0.0f;
    }

    //現在の部屋のカメラの回転移動処理
    private void TransitionCameraIn()
    {
        //時間の更新
        transTimer += Time.unscaledDeltaTime;

        //移動処理
        //タイマーを0から1の範囲に正規化して、イージング関数を適用
        float t = Mathf.Clamp01(transTimer / rotateDuration);
        t = Easing.EaseInOutCubic(t);
        roomCameraObject.transform.position = Vector3.Lerp(prevInfo.position, newInfo.position, t);
        roomCameraObject.transform.rotation = Quaternion.Slerp(prevInfo.rotation, newInfo.rotation, t);
    
        //回転完了
        if(transTimer > rotateDuration)
        {
            //現在のカメラ情報を保存
            prevInfo = newInfo;

            //終値点
            GameObject newRoom = playerData.currentRoomData.GetPlayerRoomData();
            Vector3 endPos = newRoom.transform.position;
            endPos.y += 10.0f;
            Quaternion endRotate = Quaternion.Euler(90f, 180.0f, 0.0f);
            newInfo.position = endPos;
            newInfo.rotation = endRotate;

            transitionCamera = TransitionCamera.CameraMove;

            transTimer = 0.0f;
        }
    }

    //現在の部屋から次の部屋へのカメラの移動処理
    private void TransitionCameraMove()
    {
        transTimer += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(transTimer / moveDuration);
        t = Easing.EaseInOutCubic(t);

        roomCameraObject.transform.position = Vector3.Lerp(prevInfo.position, newInfo.position, t);
        roomCameraObject.transform.rotation = Quaternion.Slerp(prevInfo.rotation, newInfo.rotation, t);

        if (transTimer > moveDuration)
        {
            //現在の部屋を更新
            GameObject newRoom = playerData.currentRoomData.GetPlayerRoomData();
            GameObject newCamera = newRoom.transform.GetComponentInChildren<Camera>().gameObject;

            //現在のカメラ情報を保存
            prevInfo = newInfo;

            //終値点のカメラ情報を保存
            newInfo = new TransitionCameraInfo
            {
                position = newCamera.transform.position,
                rotation = newCamera.transform.rotation
            };

            //カメラ遷移の状態を進める
            transitionCamera = TransitionCamera.CameraOut;

            //時間のリセット
            transTimer = 0.0f;
        }
    }

    //次の部屋のカメラの回転移動処理
    private void TransitionCameraOut()
    {
        //時間の更新
        transTimer += Time.unscaledDeltaTime;

        //移動処理
        //タイマーを0から1の範囲に正規化して、イージング関数を適用
        float t = Mathf.Clamp01(transTimer / rotateDuration);
        t = Easing.EaseInOutCubic(t);
        roomCameraObject.transform.position = Vector3.Lerp(prevInfo.position, newInfo.position, t);
        roomCameraObject.transform.rotation = Quaternion.Slerp(prevInfo.rotation, newInfo.rotation, t);

        //回転完了
        if (transTimer > rotateDuration)
        {
            //カメラ遷移の状態を進める
            transitionCamera = TransitionCamera.End;

            //時間のリセット
            transTimer = 0.0f;
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

        Transform roomCenterTransform = currentRoom.transform.GetChild(0).Find("Center");
        if (roomCenterTransform == null) roomCenter = currentRoom.transform.position;
        else roomCenter = roomCenterTransform.position;

        roomCameraObject = currentRoom.transform.GetComponentInChildren<Camera>().gameObject;
        roomCamera = roomCameraObject.GetComponent<CS_RoomCamera>();
        roomCameraObject.GetComponent<Camera>().enabled = true;
       
        transitionCamera = TransitionCamera.None;   // カメラの遷移終了
    }

    //========================
}
