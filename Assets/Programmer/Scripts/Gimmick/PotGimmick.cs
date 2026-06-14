// == PotGimmick.cs ==
// 作者 :秋野翔太
// 更新 :2026/04/22 作成開始
//
// ポットギミック仕様
// ・Active状態のとき、命中範囲に当たり判定を
// ・当たり判定内に、敵がいた場合、攻撃力を与える
// ・Broken状態のとき、当たり判定を消す
// ・当たり判定は、ギミックの向きに応じて、
//   ギミックの前方に設置する
// ・当たり判定の大きさは、HitRangeX, HitRangeY


using Unity.VisualScripting;
using UnityEngine;

public class PotGimmick : GimmickBase
{
    [Header("アクティブ時間")]
    [Tooltip("アクティブ状態の時間"), Min(0)]
    public float activeTime;

    [Header("重力値")]
    [Tooltip("主に落下時に使用"), Min(0)]
    [SerializeField]
    private float gravity;

    [Header("DangerZone")]
    [SerializeField, Tooltip("破壊時に生成する DangerZone")]
    private CS_DangerZone dangerZone;

    [SerializeField, Tooltip("泥棒のLayer。未設定なら全レイヤー")]
    private LayerMask thiefLayer;

    private bool isFall = false;
    private float initPositionY;
    private bool isFirstUpdate = true;
    private bool isDangerZoneSpawned;
    protected override void IdleUpdate()
    {
    }

    protected override void SearchUpdate()
    {
        Debug.Log("PotGimmick: SearchUpdate called", this);
        base.SearchUpdate();
    }   

    protected override void ActiveUpdate()
    {
        if (isFirstUpdate)
        {
            isFirstUpdate = false;
            Vector2Int directionVec = GetDirectionVec();
            Vector2Int hitCheckerGridPos = new Vector2Int(gimmickGridPos.x + directionVec.x, gimmickGridPos.y + directionVec.y);
            SetHitChecker(hitCheckerGridPos.x, hitCheckerGridPos.y);

            //インタラクトされた方向に地面があるか確認
            //原点をギミックの方向にずらして、下方にレイを飛ばす
            //インタラクト時転がす
            float interactVecX = 0f;
            float interactVecZ = 0f;
            if (gimmickDirection == GimmickDirection.Up)
            {//Z+
                interactVecZ = -roomGrid.gridSize.x;
            }
            else if (gimmickDirection == GimmickDirection.Down)
            {//Z-
                interactVecZ = roomGrid.gridSize.x;
            }
            else if (gimmickDirection == GimmickDirection.Left)
            {//X-
                interactVecX = roomGrid.gridSize.x;
            }
            else if (gimmickDirection == GimmickDirection.Right)
            {//X+
                interactVecX = -roomGrid.gridSize.x;
            }
            Vector3 originPos = transform.position + new Vector3(interactVecX, transform.position.y, interactVecZ);
            initPositionY = originPos.y;
            isFall = true;
            //落下地点に移動
            originPos.y = transform.position.y;
            transform.position = originPos;
        }

        //落下
        transform.position -= new Vector3(0, gravity, 0);
        SetHitChecker(transform.position);

        Ray ray = new Ray(transform.position, Vector3.down);
        if (!Physics.Raycast(ray, out RaycastHit hit, initPositionY + 0.1f))
        {//初期の高さ+aのレイを出しをそれが当たっていなかったら破壊
            gimmickState = GimmickState.Broken;
        }

        activeTime -= Time.deltaTime;
        if (activeTime <= 0)
        {
            gimmickState = GimmickState.Broken;
        }
    }

    protected override void BrokenUpdate()
    {
        base.BrokenUpdate();
        //破壊時に1回だけ生成
        if (!isDangerZoneSpawned)
        {
            isDangerZoneSpawned = true;

            if (dangerZone != null)
            {
                // ThiefCommonDBから残存時間を取得
                CO_ThiefCommonStatusData common = null;
                var thiefManager = GameObject.FindObjectOfType<CS_ThiefManager>();
                if (thiefManager != null) common = thiefManager.GetThiefCommonDB();
                CS_DangerZoneSpawner.SpawnAndRegisterFromGimmick(dangerZone, transform.position, this, common, thiefLayer);
            }
            else
            {
                Debug.LogWarning("PotGimmick: dangerZone が未設定です。", this);
            }
        }
        if(gimmickSound != null)gimmickSound.PlayOneShotSE("PotFall", gameObject.transform.position, "PotSound");
        isFirstUpdate = true;
        DeleteHitChecker();
    }
}
