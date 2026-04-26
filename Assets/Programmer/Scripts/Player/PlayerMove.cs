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
    [Header("移動速度（歩き）")][SerializeField] private float velocityWalk = 0.7f;//移動速度（歩き）
    [Header("移動速度（走り）")][SerializeField] private float velocityRun = 1.0f;//移動速度（走り）
    [Header("ジャンプ量")][SerializeField] private float jumpAmount = 2.5f;//ジャンプ量

    public PlayerInput playerInput { private set; get; }

    private float accelartion = 10;//加速度

    private Rigidbody rb;

    private void Awake()
    {
        playerInput = new PlayerInput();
        playerInput.Player.Enable();
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnDestroy()
    {
        playerInput.Player.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        //カメラの方向
        PlayerCamera playerCamera = GetComponent<PlayerCamera>();
        Vector3 forward = playerCamera.cameraForward;
        Vector3 right = playerCamera.cameraRight;
        Vector2 forwardXZ = new Vector2(forward.x, forward.z);
        Vector2 rightXZ = new Vector2(right.x, right.z);
        forwardXZ = forwardXZ.normalized;
        rightXZ = rightXZ.normalized;
        forward = new Vector3(forwardXZ.x, 0.0f, forwardXZ.y);
        right = new Vector3(rightXZ.x, 0.0f, rightXZ.y);

        //移動
        if (playerInput.Player.MoveForward.IsPressed())
        {
            rb.AddForce(new Vector3(forward.x,0.0f, forward.z) * accelartion, ForceMode.Acceleration);
        }
        else if (playerInput.Player.MoveBack.IsPressed())
        {
            rb.AddForce(new Vector3(-forward.x, 0.0f, -forward.z) * accelartion, ForceMode.Acceleration);
        }

        if (playerInput.Player.MoveLeft.IsPressed())
        {
            rb.AddForce(new Vector3(-right.x, 0.0f, -right.z) * accelartion, ForceMode.Acceleration);
        }
        else if (playerInput.Player.MoveRight.IsPressed())
        {
            rb.AddForce(new Vector3(right.x, 0.0f, right.z) * accelartion, ForceMode.Acceleration);
        }

        if (playerInput.Player.Dash.IsPressed())
        {
            //走り
            if (rb.velocity.magnitude > moveAmount * velocityRun)
            {
                rb.velocity = rb.velocity.normalized * (moveAmount * velocityRun);
            }   
        }
        else
        {
            //歩き
            if (rb.velocity.magnitude > moveAmount * velocityWalk)
            {
                rb.velocity = rb.velocity.normalized * (moveAmount * velocityWalk);
            }
        }

        //ジャンプ
        if(playerInput.Player.Jump.triggered)
        {
            rb.AddForce(new Vector3(0.0f, 1.0f, 0.0f) * jumpAmount, ForceMode.Impulse);
        }

    }
}
