using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("基礎の移動量")][SerializeField] private float moveAmount = 10.0f;//移動量
    [Header("移動速度（歩き）")][SerializeField] private float velocityWalk = 0.7f;//移動速度（歩き）
    [Header("移動速度（走り）")][SerializeField] private float velocityRun = 1.0f;//移動速度（走り）
    [Header("ジャンプ量")][SerializeField] private float jumpAmount = 2.5f;//ジャンプ量

    private float accelartion = 10;//加速度

    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
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
        if (Input.GetKey(KeyCode.W))
        {
            rb.AddForce(new Vector3(forward.x,0.0f, forward.z) * accelartion, ForceMode.Acceleration);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            rb.AddForce(new Vector3(-forward.x, 0.0f, -forward.z) * accelartion, ForceMode.Acceleration);
        }

        if (Input.GetKey(KeyCode.A))
        {
            rb.AddForce(new Vector3(-right.x, 0.0f, -right.z) * accelartion, ForceMode.Acceleration);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            rb.AddForce(new Vector3(right.x, 0.0f, right.z) * accelartion, ForceMode.Acceleration);
        }

        if (Input.GetKey(KeyCode.LeftShift))
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
        if(Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(new Vector3(0.0f, 1.0f, 0.0f) * jumpAmount, ForceMode.Impulse);
        }

    }
}
