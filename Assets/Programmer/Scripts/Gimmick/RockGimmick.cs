/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    大岩ギミック
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    大瀧蓮
 * ----------------------------------------------------------
 * 2026-04-26 | 初回作成(大瀧)
 * 2026-05-08 | リファクタリング(大瀧)
 */

using UnityEngine;

public class RockGimmick : GimmickBase
{
    private bool isFirstActive = true;

    private Vector3 velocity = Vector3.zero;
    private GameObject checker;

    private float slopeAngleLimit = 10f;    //破壊判定がおこる斜面の角度限度値

    private float initPositionY;  //初期位置Y

    [Header("下方向へのレイの距離")]
    [SerializeField]
    private float rayDownLength = 1.2f;  //下方へのレイ
    [Header("前後左右へのレイの距離")]
    [SerializeField]
    private float raySideLength = 0.6f;  //前後左右へのレイ
    [Header("滑り係数")]
    [SerializeField]
    private float slideSpeed = 1f;       // 滑る強さ
    [Header("重力値")]
    [SerializeField]
    private float gravity = 2f;          // 重力
    [Header("平面の転がり速度")]
    [SerializeField]
    private float rollSpeed = 0.2f;       // 平面の転がり速度

    protected override void IdleUpdate()
    {
        initPositionY = transform.position.y;
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

            initPositionY = transform.position.y;
            velocity = Vector3.zero;
            checker = Instantiate(hitCheckerPrefab);

            // Trigger化（重要）
            Collider col = checker.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        Hit();  //ヒットチェック

        // =========================
        // 斜面滑り
        // =========================
        RaycastHit hit;
        RaycastHit check;

        // レイ起点を少し上にずらして自身コライダへの衝突を回避する
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        bool hasValidHit = false;   //ヒットが有効かどうか

        // 下方向へのレイキャスト　※落下時の判定用
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, rayDownLength, ~0, QueryTriggerInteraction.Ignore))
        {
            // 自身または子に当たっているなら RaycastAll で次のヒットを探す
            if (hit.collider != null && (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform)))
            {
                var hits = Physics.RaycastAll(rayOrigin, Vector3.down, rayDownLength);
                foreach (var h in hits)
                {
                    //例外用処理
                    if (h.collider == null) continue;
                    if (h.collider.gameObject == gameObject) continue;
                    if (h.collider.transform.IsChildOf(transform)) continue;
                    
                    hit = h;
                    hasValidHit = true;
                    break;
                }
            }
            else
            {
                hasValidHit = true;
            }

            //インタラクト時転がす
            if (gimmickDirection == GimmickDirection.Up)
            {//Z+
                velocity = Vector3.back * rollSpeed;
            }
            else if (gimmickDirection == GimmickDirection.Down)
            {//Z-
                velocity = Vector3.forward * rollSpeed;
            }
            else if (gimmickDirection == GimmickDirection.Left)
            {//X-
                velocity = Vector3.right * rollSpeed;
            }
            else if (gimmickDirection == GimmickDirection.Right)
            {//X+
                velocity = Vector3.left * rollSpeed;
            }
            transform.position += velocity * Time.deltaTime;
        }

        if (hasValidHit)
        {//地面接触時
            Vector3 normal = hit.normal;
            Vector3 slopeDir = Vector3.ProjectOnPlane(Vector3.down, normal);

            //---------------
            // 地面判定
            float angle = Vector3.Angle(normal, Vector3.up);
            float speed = Mathf.Sin(angle * Mathf.Deg2Rad) * slideSpeed;
            if (angle < slopeAngleLimit && transform.position.y < initPositionY - 0.2f/*落下判定の距離*/)
            {
                //接地判定
                //接地(滑らない床)は破壊※一定以上落下している場合のみ
                Hit();
                gimmickState = GimmickState.Broken;
            }

            Vector3 pos = transform.position;
            // 滑り
            pos += slopeDir * speed * Time.deltaTime;

            //---------------
            // 壁判定
            //XZ方向にレイを飛ばす
            Vector3 flatForward = slopeDir;
            flatForward.y = 0f;
            //レイデバッグ
            Debug.DrawRay(transform.position, flatForward.normalized * raySideLength, Color.yellow);
            //レイ判定
            if (Physics.Raycast(transform.position, flatForward.normalized, out check, raySideLength))
            {//レイが当たったら角度をチェック
                if (HitBrokeAngle(check, flatForward, slopeAngleLimit))
                {//当たった面が一定値以上の斜面なら
                    Hit();
                    gimmickState = GimmickState.Broken;
                }
            }

            // Yだけ補正
            pos.y = hit.point.y + 0.5f;
            transform.position = pos;
        }
        else
        {
            // =========================
            // 落下
            // =========================
            velocity.y -= gravity * Time.deltaTime;
            transform.position += velocity * Time.deltaTime;
        }
    }

    // =========================
    // ヒット処理
    // =========================
    private void Hit()
    {
        SetHitChecker(gimmickGridPos.x, gimmickGridPos.y);
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
    // =========================
    // 破壊処理
    // =========================
    protected override void BrokenUpdate()
    {
        DeleteHitChecker();

        if (checker != null)
            Destroy(checker);

        Destroy(gameObject);
    }
}
