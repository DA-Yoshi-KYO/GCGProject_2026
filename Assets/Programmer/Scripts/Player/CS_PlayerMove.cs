/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    プレイヤー移動作成
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-04-24 | 初回作成
 * 2026-05-27 | リファクタリング（吉田）
 * 2026-05-29 | 足跡の生成処理追加
 */
using UnityEngine;
using UnityEngine.InputSystem;

public class CS_PlayerMove : MonoBehaviour
{
    // 移動用ステータス
    [Header("基礎の移動量")][SerializeField] private float moveAmount = 10.0f; //基礎移動量
    [Header("移動速度(歩き)")][SerializeField]private float velocityWalk = 1.0f;  //移動速度(歩き)
    [Header("移動速度（走り）")][SerializeField] private float velocityRun = 1.5f;//移動速度(走り)
    [Header("ジャンプ量")][SerializeField] private float jumpAmount = 2.5f;//ジャンプ量
    [Header("重力")][SerializeField] private float gravity = -9.8f;//重力
    [Header("空気抵抗")][Range(0, 1)][SerializeField] private float airResistance = 0.99f;//空気抵抗
    
    private CharacterController controller; // CharacterController(移動用)
    private Rigidbody rb;                   // Rigidbody(回転用)
    private Vector2 inputDirection = Vector2.zero;  // 入力された移動方向
    private Vector3 velocity = Vector3.zero;        // 現在の移動速度
    private CS_PlayerData playerData;       // プレイヤーのデータ
    private CS_PlayerCamera playerCamera;   // プレイヤーのカメラ
    private CS_3DPlaySE playSE;

    // ロジックと見た目を揃える為の保存変数
    public Transform visualModel;       // 見た目のモデル
    public Vector3 previousPosition;    // 前回の位置
    public Vector3 currentPosition;     // 現在の位置
    public Quaternion previousRotation; // 前回の回転
    public Quaternion currentRotation;  // 現在の回転
    
    private float rotateSpeed = 20.0f;  // 回転のスピード
    private bool isJumping = false;     // ジャンプ中かどうか
    private bool isRunning = false;    // スニーク中かどうか

    private bool isInvincible = false;  // 無敵状態かどうか
    public bool IsInvincible => isInvincible; // 無敵状態かどうかの取得

    private CS_FootPrint footPrint;
    private float createFootPrintTime = 100.0f;

    private Animator animator; // プレイヤーのアニメーター
    Material[] materials; // プレイヤーのマテリアル配列

    [Tooltip("盗賊に捕まっているかどうか")]
    float catCaughtTime = 0.0f;
    float invincibleTime = 0.0f;

    //スタン状態はどうか
    float ankhStunTimeToCatStun = 0.0f;

    [Header("ジャンプ開始するまでのマージン(秒数単位)")]
    [SerializeField] private float jumpMerginDuration;
    private float jumpMerginTimer;
    private bool isJumpMerging = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        footPrint = GetComponent<CS_FootPrint>();

        if (rb == null)
        {
            Debug.LogError("Rigitbodyコンポーネントが見つかりませんでした。");
            return;
        }
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogError("CharacterControllerコンポーネントが見つかりませんでした。");
            return;
        }
        playerData = GetComponent<CS_PlayerData>();
        if (playerData == null)
        {
            Debug.LogError("CS_PlayerDataコンポーネントが見つかりませんでした。");
            return;
        }
        playerCamera = GetComponent<CS_PlayerCamera>();
        if (playerCamera == null) Debug.LogError("CS_PlayerCameraコンポーネントが見つかりませんでした。");

        // インプットアクションの登録
        // 移動
        playerData.customInputAction.Player.Move.started += OnMove;
        playerData.customInputAction.Player.Move.performed += OnMove;
        playerData.customInputAction.Player.Move.canceled += OnMove;

        // ジャンプ
        playerData.customInputAction.Player.Jump.started += OnJump;

        // スニーク
        playerData.customInputAction.Player.Sneak.started += OnSneak;
        playerData.customInputAction.Player.Sneak.performed += OnSneak;
        playerData.customInputAction.Player.Sneak.canceled += OnSneak;

        // アニメーターの取得
        animator = GetComponentInChildren<Animator>();
        jumpMerginTimer = jumpMerginDuration;

        GameObject se3DObject = GameObject.Find("3DSE");
        playSE = se3DObject != null ? se3DObject.GetComponent<CS_3DPlaySE>() : null;

        if (playSE == null)
        {
            Debug.LogWarning("[PlayerMove] 3DSE が見つかりません。SE再生は無効になります。");
        }

        currentPosition = transform.position;
        previousPosition = currentPosition;
        currentRotation = rb.rotation;
        previousRotation = currentRotation;

        materials = GetComponentInChildren<SkinnedMeshRenderer>().materials;
    }

    void FixedUpdate()
    {
        // ゲームが一時停止中の場合は移動処理を行わない
        if (Time.timeScale == 0) return;

        invincibleTime -= Time.fixedDeltaTime;
        if (invincibleTime <= 0.0f && isInvincible)
        {
            isInvincible = false;
            invincibleTime = 0.0f;

            foreach (var material in materials)
            {
                material.SetFloat("_Alpha", 1f);
            }
        }

        createFootPrintTime += Time.fixedDeltaTime;
        previousPosition = currentPosition;
        previousRotation = currentRotation;

        Move();

        if (isJumpMerging)
        {
            if (jumpMerginTimer <= 0f)
            {
                isJumping = true;
                isJumpMerging = false;
                jumpMerginTimer = jumpMerginDuration;
            }
            else jumpMerginTimer -= Time.fixedDeltaTime;
        }

        currentPosition = transform.position; // CharacterController.Move後の実位置
        currentRotation = rb.rotation;
    }

    void Update()
    {
        // FixedUpdate間の経過割合を計算
        float t = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
        t = Mathf.Clamp01(t);

        // 見た目だけ補間して滑らかに動かす
        visualModel.position = Vector3.Lerp(previousPosition, currentPosition, t);
        visualModel.rotation = Quaternion.Slerp(previousRotation, currentRotation, t);
    }

    /// <summary>
    /// 移動処理
    /// </summary>
    private void Move()
    {
        if (catCaughtTime > 0f)
        {
            catCaughtTime -= Time.fixedDeltaTime;
            catCaughtTime = Mathf.Max(0.0f, catCaughtTime);
            return;
        }

        if (ankhStunTimeToCatStun > 0.0f)
        {//猫がスタンしている場合動かせない
            ankhStunTimeToCatStun -= Time.fixedDeltaTime;
            ankhStunTimeToCatStun = Mathf.Max(0.0f, ankhStunTimeToCatStun);
            return;
        }

        if (isInvincible)
        {
            float sineValue = Mathf.Abs(Mathf.Sin(invincibleTime * 180f * Mathf.Deg2Rad));
            foreach (var material in materials)
            {
                material.SetFloat("_Alpha", sineValue);
            }
        }

        // ジャンプ待機中は移動処理を行わない
        if (isJumpMerging && !isJumping) return;


        // カメラの前方向と右方向を取得し、y成分を0にして水平移動のベクトルを作成
        Vector3 cameraForward = playerCamera.cameraForward;
        Vector3 cameraRight = playerCamera.cameraRight;
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // 入力された移動量に基づいて速度を計算
        float speed = (moveAmount) * (isRunning ? velocityRun : velocityWalk);

        // 入力を成分ごとに分解
        Vector3 forwardMove = cameraForward * (inputDirection.y * speed);
        Vector3 rightMove = cameraRight * (inputDirection.x * speed);

        Vector3 horizontalMove; // 最終的な水平移動ベクトル
        horizontalMove = forwardMove + rightMove;   // それ以外は入力された移動をそのまま使用

        // 移動量を更新
        velocity = new Vector3(horizontalMove.x, velocity.y, horizontalMove.z);

        // 水平方向の移動がある場合は、プレイヤーを移動方向に回転させる
        if (horizontalMove.sqrMagnitude > 0.0001f)
        {
            // プレイヤーの回転を移動方向に向ける
            Quaternion playerRotate = Quaternion.LookRotation(horizontalMove);
            // Rigidbodyを使用して滑らかに回転させる
            rb.MoveRotation(Quaternion.Slerp(
                rb.rotation, playerRotate, Time.fixedDeltaTime * rotateSpeed));
        }

        // 接地
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -1f;
            isJumping = false;
        }

        // 上昇終了
        if (isJumping && velocity.y <= 0)
        {
            isJumping = false;
        }

        // 上昇中の空気抵抗
        if (isJumping)
        {
            velocity.y *= airResistance;
        }

        //足跡の生成
        if (velocity != Vector3.zero && controller.isGrounded)
        {
            if (createFootPrintTime > footPrint.createFootPrintDuration)
            {
                createFootPrintTime = 0.0f;
                footPrint.SpawnFootprintAuto();
            }
        }

        // 重力
        velocity.y += gravity * Time.fixedDeltaTime;

        // CharacterControllerを使用して移動
        controller.Move(velocity * Time.fixedDeltaTime);

        Vector2 velocityXZ = new Vector2(velocity.x, velocity.z);
        bool isMoving = velocityXZ.sqrMagnitude > 0.0001f;
        if (isMoving && isRunning && controller.isGrounded)
        {
            animator.speed = 1.5f;
        }
        else
        {
            animator.speed = 1.0f;
        }
        animator.SetBool("IsGround", controller.isGrounded);
        animator.SetBool("IsMoving", isMoving);
    }



    /// <summary>
    /// 部屋移動やワープなど、外部からTransformを直接書き換えた際に
    /// Rigidbodyと補間用の前回/現在値、見た目のモデルを同期させる関数。
    /// これを呼ばずにtransformだけ書き換えると、次のFixedUpdateで
    /// currentRotationがrb.rotationの古い値で上書きされ、回転が一瞬戻る
    /// がたつきが発生する。
    /// </summary>
    /// <param name="position">同期後の位置</param>
    /// <param name="rotation">同期後の回転</param>
    public void SyncTransform(Vector3 position, Quaternion rotation)
    {
        rb.position = position;
        rb.rotation = rotation;

        previousPosition = position;
        currentPosition = position;
        previousRotation = rotation;
        currentRotation = rotation;

        visualModel.SetPositionAndRotation(position, rotation);
    }

    /// <summary>
    /// 基準となるプレイヤーの移動速度を取得します。
    /// </summary>
    /// <returns>プレイヤーの移動速度</returns>
    public float GetBasePlayerSpeed()
    {
        return moveAmount * velocityWalk;
    }

    /// <summary>
    /// 盗賊に捕まったときの処理
    /// </summary>
    public void CaughtByThief(float holdCatTime, Transform thiefTransform)
    {
        transform.position = new Vector3(thiefTransform.position.x, thiefTransform.position.y - thiefTransform.localScale.y / 2.0f, thiefTransform.position.z);
        transform.position += thiefTransform.forward;
        visualModel.position = transform.position;

        // フラグを立てる
        catCaughtTime = holdCatTime;
        invincibleTime = holdCatTime * 2f;
        isInvincible = true;
        playSE.PlayOneShotSE("Cat_HitThief", gameObject.transform.position, "Cat_HitThief");
    }

    // 猫のスタン状態用処理※Ankh用
    public void SetAnkhCatStunTime(float stunTime)
    {
        ankhStunTimeToCatStun = stunTime;
    }

    // ---InputActionのコールバック関数---
    private void OnMove(InputAction.CallbackContext context)
    {
        inputDirection = context.ReadValue<Vector2>();
    }
    private void OnJump(InputAction.CallbackContext context)
    {
        // 空中にいるときはジャンプできないようにする
        if (controller.isGrounded)
        {
            animator.SetTrigger("JumpTrigger");
            velocity.y = jumpAmount;
            isJumpMerging = true;
        }
    }
    private void OnSneak(InputAction.CallbackContext context)
    {
        if (context.canceled) isRunning = false;
        else isRunning = true;
    }

    /// <summary>
    /// 無敵状態のフラグを設定する
    /// </summary>
    /// <param name="isFlag">無敵状態にする場合はtrue、解除する場合はfalse</param>
    public void SetInvincibleFlag(bool isFlag)
    {
        isInvincible = isFlag;
    }
}
