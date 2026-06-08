using UnityEngine;
using UnityEngine.InputSystem;

public class CS_TimeLinePlayer : MonoBehaviour
{
    // 移動用ステータス
    [Header("基礎の移動量")][SerializeField] private float moveAmount = 10.0f; //基礎移動量
    [Header("移動速度(歩き)")][SerializeField] private float velocityWalk = 1.0f;  //移動速度(歩き)
    [Header("移動速度（走り）")][SerializeField] private float velocitySneak = 0.6f;//移動速度(スニーク)
    [Header("ジャンプ量")][SerializeField] private float jumpAmount = 2.5f;//ジャンプ量
    [Header("重力")][SerializeField] private float gravity = -9.8f;//重力
    [Header("空気抵抗")][Range(0, 1)][SerializeField] private float airResistance = 0.99f;//空気抵抗

    private CharacterController controller; // CharacterController(移動用)
    private Rigidbody rb;                   // Rigidbody(回転用)
    private Vector2 inputDirection = Vector2.zero;  // 入力された移動方向
    private Vector3 velocity = Vector3.zero;        // 現在の移動速度
    private CS_PlayerData playerData;       // プレイヤーのデータ
    [SerializeField] private Camera playerCamera;   // プレイヤーのカメラ

    private float rotateSpeed = 10.0f;  // 回転のスピード
    private bool isJumping = false;     // ジャンプ中かどうか
    private bool isSneaking = false;    // スニーク中かどうか

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null) Debug.LogError("Rigitbodyコンポーネントが見つかりませんでした。");
        controller = GetComponent<CharacterController>();
        if (controller == null) Debug.LogError("CharacterControllerコンポーネントが見つかりませんでした。");
        playerData = GetComponent<CS_PlayerData>();
        if (playerData == null) Debug.LogError("CS_PlayerDataコンポーネントが見つかりませんでした。");

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
    }

    void Update()
    {
        
        // 移動処理
        Move();
    }

    /// <summary>
    /// 移動処理
    /// </summary>
    private void Move()
    {
        // ゲームが一時停止中の場合は移動処理を行わない
        if (Time.timeScale == 0) return;

        // カメラの前方向と右方向を取得し、y成分を0にして水平移動のベクトルを作成
        Vector3 cameraForward = playerCamera.transform.forward;
        Vector3 cameraRight = playerCamera.transform.right;
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // 入力された移動量に基づいて速度を計算
        float speed = (moveAmount) * (isSneaking ? velocitySneak : velocityWalk);

        // 入力を成分ごとに分解
        Vector3 forwardMove = cameraForward * (inputDirection.y * speed);
        Vector3 rightMove = cameraRight * (inputDirection.x * speed);

        Vector3 horizontalMove; // 最終的な水平移動ベクトル
        if (isSneaking && controller.isGrounded)    // スニーク中で地面にいる場合は、移動可能な成分のみを通す
        {
            // スニーク中の移動量の計算
            horizontalMove = ResolveSneakMove(forwardMove, rightMove);
        }
        else
        {
            horizontalMove = forwardMove + rightMove;   // それ以外は入力された移動をそのまま使用

        }

        // 移動量を更新
        velocity = new Vector3(horizontalMove.x, velocity.y, horizontalMove.z);

        // 水平方向の移動がある場合は、プレイヤーを移動方向に回転させる
        if (horizontalMove.sqrMagnitude > 0.0001f)
        {
            // プレイヤーの回転を移動方向に向ける
            Quaternion playerRotate = Quaternion.LookRotation(horizontalMove);
            // Rigidbodyを使用して滑らかに回転させる
            rb.MoveRotation(Quaternion.Slerp(
                rb.rotation, playerRotate, Time.deltaTime * rotateSpeed));
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


        // 重力
        velocity.y += gravity * Time.deltaTime;

        // CharacterControllerを使用して移動
        controller.Move(velocity * Time.deltaTime);
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
    /// スニーク中は斜め入力の際に地面がない方向への移動を制限するため
    /// 成分ごとに移動可能かを判定して通すかどうかを決定する関数。
    /// </summary>
    /// <param name="forwardMove">正面方向への移動量</param>
    /// <param name="rightMove">右方向への移動量</param>
    /// <returns></returns>
    private Vector3 ResolveSneakMove(Vector3 forwardMove, Vector3 rightMove)
    {
        Vector3 accepted = Vector3.zero;    // 最終的に通す移動量

        // 斜め入力時は成分ごとに通す
        if (forwardMove.sqrMagnitude >= rightMove.sqrMagnitude)
        {
            TryAddSneakComponent(ref accepted, forwardMove);
            TryAddSneakComponent(ref accepted, rightMove);
        }
        else
        {
            TryAddSneakComponent(ref accepted, rightMove);
            TryAddSneakComponent(ref accepted, forwardMove);
        }

        return accepted;
    }

    // ある方向への移動を足した先に地面があるかを判定し、あれば移動を通す関数。
    private void TryAddSneakComponent(ref Vector3 currentMove, Vector3 addMove)
    {
        if (addMove.sqrMagnitude <= 0.0001f) return;    // 移動量がほぼゼロなら判定しない

        // 現在の移動にこの成分を足した先に地面があるかを判定
        if (HasGroundForMove(currentMove, addMove, 0.3f))
        {
            currentMove += addMove; // 地面があればこの成分を通す
        }
    }

    private bool HasGroundForMove(Vector3 currentMove, Vector3 addMove, float checkLength)
    {
        // 追加する移動の方向を取得
        Vector3 dir = addMove.normalized;

        // CharacterController の足元基準
        Vector3 feet = controller.center + transform.position;
        feet.y -= controller.height / 2;

        // 既に通っている移動を足した先で判定
        Vector3 checkPos = feet + currentMove * Time.deltaTime + dir * checkLength;
        checkPos.y += 0.1f;

        // 地面があるかをレイで判定
        RaycastHit hit;
        bool hasGround = Physics.Raycast(
            checkPos,
            Vector3.down,
            out hit,
            0.1f + 0.6f,
            ~0,
            QueryTriggerInteraction.Ignore);

        return hasGround;
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
            velocity.y = jumpAmount;
            isJumping = true;
        }
    }
    private void OnSneak(InputAction.CallbackContext context)
    {
        if (context.canceled) isSneaking = false;
        else isSneaking = true;
    }
}
