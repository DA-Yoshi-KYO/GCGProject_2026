using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class RockGimmick : GimmickBase
{
    private bool isFirstActive = true;
    private bool hasHit = false;

    private Vector3 velocity = Vector3.zero;
    private GameObject checker;

    private float timer = 0f;
    [Header("下方向へのレイの距離")]
    public float rayDownLength = 1.2f;
    [Header("前後左右へのレイの距離")]
    public float raySideLength = 0.6f;
    [Header("滑り係数")]
    public float slideSpeed = 1f;    // 滑る強さ
    [Header("重力値")]
    public float gravity = 1f;
    protected override void IdleUpdate()
    {
    }

    protected override void ActiveUpdate()
    {
        // =========================
        // 初期化
        // =========================
        if (isFirstActive)
        {
            isFirstActive = false;
            Vector2Int directionVec = GetDirectionVec();
            Vector2Int hitCheckerGridPos = new Vector2Int(gimmickGridPos.x + directionVec.x, gimmickGridPos.y + directionVec.y);

            // ワールド変換
            Vector3 world;
            world.x = hitCheckerGridPos.x;
            world.y = 50f;
            world.z = hitCheckerGridPos.y;

            // ★XZだけグリッド、Yは固定高さ
            transform.position = new Vector3(world.x, world.y, world.z);

            velocity = Vector3.zero;

            checker = Instantiate(hitCheckerPrefab);

            // Trigger化（重要）
            Collider col = checker.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        // =========================
        // 初期フレーム無視
        // =========================
        //timer += Time.deltaTime;
        //if (timer < 0.1f) return;
        Hit();
        // =========================
        // 斜面滑り
        // =========================
        RaycastHit hit;
        RaycastHit check;
        // 下にRayを飛ばす
        if (Physics.Raycast(transform.position, Vector3.down, out hit, rayDownLength))
        {
            Debug.Log("DownRayが当たった: " + hit.collider.name);

            Vector3 normal = hit.normal;

            Vector3 slopeDir = Vector3.ProjectOnPlane(Vector3.down, normal);

            float angle = Vector3.Angle(normal, Vector3.up);
            float speed = Mathf.Sin(angle * Mathf.Deg2Rad) * slideSpeed;
            if (angle < 20f)
            {
                //接地判定
                //接地(滑らない床)は破壊
                Debug.Log("地面接触：破壊");
                Hit();
                gimmickState = GimmickState.Broken;
            }
            Vector3 pos = transform.position;

            // 滑り
            pos += slopeDir * speed * Time.deltaTime;

            //XZ方向にレイを飛ばす
            Vector3 flatForward = slopeDir;
            flatForward.y = 0f;
            //レイデバッグ
            Debug.DrawRay(transform.position, flatForward.normalized * raySideLength, Color.yellow);
            //レイ判定
            if (Physics.Raycast(transform.position, flatForward.normalized, out check, raySideLength))
            {
                Debug.Log("XZ方向にヒット: " + check.collider.name);
                if (HitBrokeAngle(check, flatForward, 30f))
                {
                    Debug.Log("側面に接触：破壊");
                    Hit();
                    gimmickState = GimmickState.Broken;
                }
            }

            // Yだけ補正
            pos.y = hit.point.y + 0.5f;

            transform.position = pos;
        }

        // =========================
        // 落下
        // =========================
        velocity.y -= gravity * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;
    }

    // =========================
    // 地面判定
    // =========================
    private bool CheckGround()
    {
        Vector3 pos = transform.position + Vector3.down;

        if(pos.y <= 0f)
        {
            Debug.Log("地面に接触: " + pos);
            return true;
        }

        return false;
    }

    // =========================
    // ヒット処理
    // =========================
    private void Hit()
    {
        Vector3 pos = transform.position + Vector3.down;

        SetHitChecker((int)pos.x, (int)pos.z);
    }
    // =========================
    // レイヒットオブジェクトの角度計算
    // =========================
    private bool HitBrokeAngle(RaycastHit hit, Vector3 rayDir, float breakAngle)
    {
        Vector3 normal = hit.normal;

        // Rayは「進行方向」なので反転させる
        float angle = Vector3.Angle(normal, -rayDir);

        return angle < breakAngle;
    }
    protected override void BrokenUpdate()
    {
        DeleteHitChecker();

        if (checker != null)
            Destroy(checker);

        Destroy(gameObject);
    }
}
