/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    プレイヤー移動作成
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-04-24 | 初回作成
 * 
 */
using UnityEditor.ShaderGraph;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("基礎の移動量")][SerializeField] private float moveAmount = 10.0f;//移動量
    [Header("移動速度（歩き）")][SerializeField] private float velocityWalk = 0.7f;//移動速度（歩き）
    [Header("移動速度（走り）")][SerializeField] private float velocityRun = 1.0f;//移動速度（走り）
    [Header("ジャンプ量")][SerializeField] private float jumpAmount = 2.5f;//ジャンプ量
    [Header("加速度")][SerializeField] private float accelartion = 10;//加速度
    
    private Rigidbody rb;
    private PlayerData playerData;  // プレイヤーのデータ

    public Vector3 playerMoveAmount;//プレイヤーの移動量

    private float rotateSpeed = 10.0f;//回転のスピード

    private int jumpCount = 1;//ジャンプできる回数

    private Vector3 wallNormal;//壁の法線ベクトル
    private bool touchingWall;//壁に当たっているかどうか

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerData = GetComponent<PlayerData>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //プレイヤーの動く方向に回転
        Vector3 velocity = rb.velocity;

        velocity.y = 0.0f;

        
        if(velocity.magnitude > 0.1f)
        {
            velocity = velocity.normalized;
            Quaternion playerRotate = Quaternion.LookRotation(velocity);

            rb.MoveRotation(Quaternion.Slerp(
                rb.rotation, playerRotate, Time.fixedDeltaTime * rotateSpeed));
        }

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

        //プレイヤーの前方向
        Vector3 playerForward = transform.forward;
        
        //壁に向かっているか判定
        bool pushingIntoWall = false;
        if (touchingWall)
        {
            float inputDot = Vector3.Dot(playerForward, wallNormal);
            //プレイヤーが壁方向を向いている
            if (inputDot < 0)
            {
                pushingIntoWall = true;
            }
        }

        //移動
        if (playerData.playerInput.Player.MoveForward.IsPressed() && !pushingIntoWall)
        {
            rb.AddForce(new Vector3(forward.x,0.0f, forward.z) * accelartion, ForceMode.Acceleration);
        }
        else if (playerData.playerInput.Player.MoveBack.IsPressed() && !pushingIntoWall)
        {
            rb.AddForce(new Vector3(-forward.x, 0.0f, -forward.z) * accelartion, ForceMode.Acceleration);
        }

        if (playerData.playerInput.Player.MoveLeft.IsPressed() && !pushingIntoWall)
        {
            rb.AddForce(new Vector3(-right.x, 0.0f, -right.z) * accelartion, ForceMode.Acceleration);
        }
        else if (playerData.playerInput.Player.MoveRight.IsPressed() && !pushingIntoWall)
        {
            rb.AddForce(new Vector3(right.x, 0.0f, right.z) * accelartion, ForceMode.Acceleration);
        }

        if (playerData.playerInput.Player.Dash.IsPressed() && !pushingIntoWall)
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

        playerMoveAmount = rb.velocity;
    }

    void Update()
    {
        //ジャンプ
        if (playerData.playerInput.Player.Jump.triggered && jumpCount > 0)
        {
            rb.AddForce(new Vector3(0.0f, 1.0f, 0.0f) * jumpAmount, ForceMode.Impulse);
            jumpCount--;
        }
    }

    /// <summary>
    /// 基準となるプレイヤーの移動速度を取得します。
    /// </summary>
    /// <returns>プレイヤーの移動速度</returns>
    public float GetBasePlayerSpeed()
    {
        return moveAmount * velocityRun;
    }

    private void OnCollisionEnter(Collision collision)
    {
        //床に接地している場合ジャンプ回数を戻す
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                jumpCount = 1;
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        touchingWall = false;

        //ジャンプ中に物体に衝突した際に止まらないようにする処理
        foreach (ContactPoint contact in collision.contacts)
        {
            Vector3 normal = contact.normal;

            //床は処理しない
            if (normal.y > 0.8f)
                continue;

            //壁に触れている
            touchingWall = true;
            wallNormal = normal;

            Vector3 velocity = rb.velocity;

            //壁に向かう成分
            float wallSeekingComponent = Vector3.Dot(velocity, normal);

            if (wallSeekingComponent < 0.0f)
            {
                //壁方向の力だけ削除
                velocity -= normal * wallSeekingComponent;
            }

            rb.velocity = velocity;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        touchingWall = false;
    }
}
