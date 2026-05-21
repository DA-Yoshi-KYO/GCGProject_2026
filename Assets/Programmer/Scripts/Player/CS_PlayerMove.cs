/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    プレイヤー移動作成
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-04-24 | 初回作成
 * 
 */
using UnityEngine;
using UnityEngine.InputSystem;

public class CS_PlayerMove : MonoBehaviour
{
    [Header("基礎の移動量")][SerializeField] private float moveAmount = 10.0f;//移動量
    private float velocityWalk = 1.0f;//移動速度（歩き）
    [Header("移動速度（走り）")][SerializeField] private float velocitySneak = 0.6f;//移動速度（走り）
    [Header("ジャンプ量")][SerializeField] private float jumpAmount = 2.5f;//ジャンプ量
    [Header("重力")][SerializeField] private float gravity = -9.8f;//重力
    [Header("空気抵抗")][Range(0, 1)][SerializeField] private float airResistance = 0.99f;//空気抵抗
    [Header("加速度")][SerializeField] private float accelartion = 10;//加速度

    private CharacterController controller;
    private Vector2 inputDirection = Vector2.zero;
    private Vector3 velocity = Vector3.zero;
    private Rigidbody rb;
    private PlayerData playerData;  // プレイヤーのデータ
    private CS_PlayerCamera playerCamera;  // プレイヤーのカメラ

    private float rotateSpeed = 10.0f;//回転のスピード
    private float adjustControllerSpeed = 1;//移動スピードの補正
    private bool isJumping = false;
    private bool isSneaking = false;

    private float audioTime = 100.0f;

    private CS_3DPlaySE playSE3D;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerData = GetComponent<PlayerData>();
        playerCamera = GetComponent<CS_PlayerCamera>();
        controller = GetComponent<CharacterController>();

        playSE3D = GameObject.Find("3DSE").GetComponent<CS_3DPlaySE>();

        // インプットアクションの登録
        playerData.customInputAction.Player.Move.started += OnMove;
        playerData.customInputAction.Player.Move.performed += OnMove;
        playerData.customInputAction.Player.Move.canceled += OnMove;

        playerData.customInputAction.Player.Jump.started += OnJumo;
    }

    void Update()
    {
        Move();
    }

    private void Move()
    {
        if (Time.timeScale == 0) return;

        //移動の足音の処理
        if(inputDirection.x != 0.0f || inputDirection.y != 0.0f)
        {
            if (playSE3D.GetAudioLength("PlayerWalkNormal") < audioTime)
            {
                audioTime = 0.0f;
                playSE3D.PlayOneShotSE("PlayerWalkNormal", transform.position, "Walk", CS_3DPlaySE.SEMode.Normal);
            }
        }

        Vector3 cameraForward = playerCamera.cameraForward;
        Vector3 cameraRight = playerCamera.cameraRight;
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        float speed = (moveAmount * adjustControllerSpeed) * (isSneaking ? velocitySneak : velocityWalk);

        // 入力を成分ごとに分解
        Vector3 forwardMove = cameraForward * (inputDirection.y * speed);
        Vector3 rightMove = cameraRight * (inputDirection.x * speed);

        Vector3 horizontalMove;

        audioTime += Time.deltaTime;

        if (isSneaking && controller.isGrounded)
        {
            horizontalMove = ResolveSneakMove(forwardMove, rightMove);
        }
        else
        {
            horizontalMove = forwardMove + rightMove;
        }

        velocity = new Vector3(horizontalMove.x, velocity.y, horizontalMove.z);

        if (horizontalMove.sqrMagnitude > 0.0001f)
        {
            Quaternion playerRotate = Quaternion.LookRotation(horizontalMove);

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
   
    private bool CheckDownGround(Vector3 dir, float length)
    {
        Vector3 checkPos = transform.position + dir * length;

        checkPos.y += 0.1f;

        RaycastHit hit;
        bool hasGround = Physics.Raycast(
            checkPos,
            Vector3.down,
            out hit,
            255f,
            ~0,
            QueryTriggerInteraction.Ignore);
        
        
        bool isDown = transform.position.y - hit.point.y > 0.6f;
        Debug.DrawRay(
            checkPos,
            Vector3.down * 255f,
            isDown ? Color.green : Color.red);

        return isDown;
    }

    private Vector3 ResolveSneakMove(Vector3 forwardMove, Vector3 rightMove)
    {
        Vector3 accepted = Vector3.zero;

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

    private void TryAddSneakComponent(ref Vector3 currentMove, Vector3 addMove)
    {
        if (addMove.sqrMagnitude <= 0.0001f) return;

        if (HasGroundForMove(currentMove, addMove, 0.3f))
        {
            currentMove += addMove;
        }
    }

    private bool HasGroundForMove(Vector3 currentMove, Vector3 addMove, float checkLength)
    {
        Vector3 dir = addMove.normalized;

        // CharacterController の足元基準
        Vector3 feet = controller.center + transform.position;
        feet.y -= controller.height / 2;

        // 既に通っている移動を足した先で判定
        Vector3 checkPos = feet + currentMove * Time.deltaTime + dir * checkLength;
        checkPos.y += 0.1f;

        RaycastHit hit;
        bool hasGround = Physics.Raycast(
            checkPos,
            Vector3.down,
            out hit,
            0.1f + 0.6f,
            ~0,
            QueryTriggerInteraction.Ignore);

        Debug.DrawRay(
            checkPos,
            Vector3.down * (0.1f + 0.6f),
            hasGround ? Color.red : Color.green);

        return hasGround;
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        inputDirection = context.ReadValue<Vector2>();
    }

    private void OnJumo(InputAction.CallbackContext context)
    {
        if (controller.isGrounded)
        {
            velocity.y = jumpAmount;
            isJumping = true;
        }
    }

    private void OnSneak(InputAction.CallbackContext context)
    {
        isSneaking = playerData.customInputAction.Player.Sneak.IsPressed();
    }
}
