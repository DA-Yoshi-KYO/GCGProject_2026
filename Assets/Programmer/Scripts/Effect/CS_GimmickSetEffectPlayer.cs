using UnityEngine;

/*
+=====================================
 ファイル名 : CS_GimmickSetEffectPlayer.cs
 概要     : ギミック設置時のEffect再生クラス
 作者     : ヨシモト リョウ
 履歴     : 2026/06/11 新規作成
=====================================+
*/

/// <summary>
/// ギミック設置時のEffect再生を担当するクラスです。
/// CS_PlayerActionからEffect処理を分離します。
/// </summary>
public class CS_GimmickSetEffectPlayer : MonoBehaviour
{
    private const string PLANE_TAG = "Plane";
    private const float RAY_START_DOWN_OFFSET = 1.0f;
    private const float RAY_UP_DISTANCE = 10.0f;
    private const float EFFECT_PLANE_UP_OFFSET = 0.03f;

    [Header("ギミック設置時に再生するEffectPrefab")]
    [SerializeField]
    private GameObject go_GimmickSetEffectPrefab;

    /// <summary>
    /// ギミック設置Effect用の親Transformです。
    /// PlayerやGimmickの子にしないことで、移動の影響を受けないようにします。
    /// </summary>
    private Transform tr_GimmickSetEffectRoot;

    /// <summary>
    /// ギミック設置時のEffectを再生します。
    /// </summary>
    /// <param name="v3_SetPosition">ギミック設置位置。</param>
    /// <param name="gimmick">設置されたギミック。</param>
    public void PlayGimmickSetEffect(
        Vector3 v3_SetPosition,
        GimmickBase gimmick)
    {
        if (go_GimmickSetEffectPrefab == null)
        {
            Debug.LogWarning("[CS_GimmickSetEffectPlayer] EffectPrefabが設定されていません。");
            return;
        }

        if (gimmick == null)
        {
            return;
        }

        Vector3 v3_EffectPosition =
            GetEffectPositionOnPlane(v3_SetPosition);

        Quaternion q_EffectRotation =
            go_GimmickSetEffectPrefab.transform.rotation;

        CSAD_EffectCommonProcessBase csad_Effect =
            CS_EffectFactory.CreateEffect(
                go_GimmickSetEffectPrefab,
                v3_EffectPosition,
                q_EffectRotation,
                GetGimmickSetEffectRoot());

        if (csad_Effect == null)
        {
            return;
        }

        csad_Effect.SetOnEffectEndAction(DestroyEffect);

        CSST_EffectPlayData csst_EffectPlayData = new CSST_EffectPlayData();
        csst_EffectPlayData.CSST_EffectPlayData_Init();

        csst_EffectPlayData.SetPosition(v3_EffectPosition);
        csst_EffectPlayData.SetRotation(q_EffectRotation);

        csad_Effect.PlayEffect(csst_EffectPlayData);
    }

    /// <summary>
    /// Planeタグの床より少し上のEffect再生位置を取得します。
    /// </summary>
    /// <param name="v3_SetPosition">ギミック設置位置。</param>
    /// <returns>Effect再生位置。</returns>
    private Vector3 GetEffectPositionOnPlane(Vector3 v3_SetPosition)
    {
        Vector3 v3_RayStart = new Vector3(
            v3_SetPosition.x,
            v3_SetPosition.y - RAY_START_DOWN_OFFSET,
            v3_SetPosition.z);

        RaycastHit[] hits = Physics.RaycastAll(
            v3_RayStart,
            Vector3.up,
            RAY_UP_DISTANCE,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);

        if (hits != null && hits.Length > 0)
        {
            System.Array.Sort(
                hits,
                (a, b) => a.distance.CompareTo(b.distance));

            for (int i = 0 ; i < hits.Length ; i++)
            {
                if (hits[i].collider == null)
                {
                    continue;
                }

                if (IsPlaneHit(hits[i].collider.transform) == false)
                {
                    continue;
                }

                return hits[i].point + Vector3.up * EFFECT_PLANE_UP_OFFSET;
            }
        }

        return v3_SetPosition + Vector3.up * EFFECT_PLANE_UP_OFFSET;
    }

    /// <summary>
    /// HitしたColliderか親階層にPlaneタグがあるか確認します。
    /// </summary>
    /// <param name="tr_Target">確認対象Transform。</param>
    /// <returns>Planeタグがある場合はtrue。</returns>
    private bool IsPlaneHit(Transform tr_Target)
    {
        Transform tr_Current = tr_Target;

        while (tr_Current != null)
        {
            if (tr_Current.CompareTag(PLANE_TAG))
            {
                return true;
            }

            tr_Current = tr_Current.parent;
        }

        return false;
    }

    /// <summary>
    /// Effect終了時に破棄します。
    /// </summary>
    /// <param name="csad_Effect">終了したEffect。</param>
    private void DestroyEffect(CSAD_EffectCommonProcessBase csad_Effect)
    {
        if (csad_Effect == null)
        {
            return;
        }

        Destroy(csad_Effect.gameObject);
    }

    /// <summary>
    /// ギミック設置Effect用のRootを取得します。
    /// </summary>
    /// <returns>Effect用Root。</returns>
    private Transform GetGimmickSetEffectRoot()
    {
        if (tr_GimmickSetEffectRoot != null)
        {
            return tr_GimmickSetEffectRoot;
        }

        GameObject go_Root = GameObject.Find("GimmickSetEffectRoot");

        if (go_Root == null)
        {
            go_Root = new GameObject("GimmickSetEffectRoot");
        }

        tr_GimmickSetEffectRoot = go_Root.transform;

        return tr_GimmickSetEffectRoot;
    }
}
