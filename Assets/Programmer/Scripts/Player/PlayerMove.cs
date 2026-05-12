/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    プレイヤー移動作成
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-04-24 | 初回作成
 * 
 */
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("基礎の移動量")][SerializeField] private float moveAmount = 10.0f;//移動量
    private float velocityWalk = 1.0f;//移動速度（歩き）
    [Header("移動速度（走り）")][SerializeField] private float velocitySneak = 0.6f;//移動速度（走り）
    [Header("ジャンプ量")][SerializeField] private float jumpAmount = 2.5f;//ジャンプ量
    [Header("重力")][SerializeField] private float gravity = -9.8f;//重力
    [Header("空気抵抗")][Range(0, 1)][SerializeField] private float airResistance = 0.99f;//空気抵抗
    [Header("加速度")][SerializeField] private float accelartion = 10;//加速度

    private CharacterController controller;
    private Vector3 velocity = Vector3.zero;
    private Rigidbody rb;
    private PlayerData playerData;  // プレイヤーのデータ

    private float rotateSpeed = 10.0f;//回転のスピード
    private float adjustControllerSpeed = 1;//移動スピードの補正
    private bool isJumping = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerData = GetComponent<PlayerData>();
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        Move();
    }

    private void Move()
    {
        if (Time.timeScale == 0) return;

        bool isSneaking = playerData.playerInput.Player.Sneak.IsPressed();
        // 移動
        float h = 0.0f;
        float v = 0.0f;
        //移動
        if (playerData.playerInput.Player.MoveForward.IsPressed()) v = 1.0f;
        else if (playerData.playerInput.Player.MoveBack.IsPressed()) v = -1.0f;

        if (playerData.playerInput.Player.MoveRight.IsPressed()) h = 1.0f;
        else if (playerData.playerInput.Player.MoveLeft.IsPressed()) h = -1.0f;


        //カメラの方向
        PlayerCamera playerCamera = GetComponent<PlayerCamera>();
        // カメラ方向取得
        Vector3 cameraForward = playerCamera.cameraForward;
        Vector3 cameraRight = playerCamera.cameraRight;
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 move = cameraForward * v + cameraRight * h;
        move *= (moveAmount * adjustControllerSpeed) * (isSneaking ? velocitySneak : velocityWalk);

        velocity = new Vector3(move.x, velocity.y, move.z);

        if (move != Vector3.zero)
        {
            Quaternion playerRotate = Quaternion.LookRotation(move);

            rb.MoveRotation(Quaternion.Slerp(
                rb.rotation, playerRotate, Time.deltaTime * rotateSpeed));
        }

        // ジャンプ
        if (controller.isGrounded &&
            velocity.y < 0)
        {
            velocity.y = -1f;

            isJumping = false;
        }

        // ジャンプ開始
        if (Input.GetKeyDown(KeyCode.Space) &&
            controller.isGrounded)
        {
            velocity.y =
                Mathf.Sqrt(
                    (jumpAmount * adjustControllerSpeed) *
                    -2f *
                    gravity);

            isJumping = true;
        }

        // 上昇終了（頂点）
        if (isJumping &&
            velocity.y <= 0)
        {
            isJumping = false;
        }

        // ジャンプ上昇中だけ空気抵抗
        if (isJumping)
        {
            velocity.y *= airResistance;
        }

        // 重力
        velocity.y +=
            gravity * Time.deltaTime;

        Vector3 dir = velocity;
        dir.y = 0.0f;
        dir.Normalize();

        if (isSneaking && CheckDownGround(dir, 0.3f) && controller.isGrounded)
        {
            velocity.x = 0f;
            velocity.z = 0f;

            // 崖端から離れる方向に少し押し戻す
            Vector3 pushBack = -dir.normalized * 0.02f;
            controller.Move(pushBack); // めり込み解消
            return;
        }

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
}
