using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

/*==================================================
 *  ファイル名  : CS_RoomCreatePoint.cs
 *  制作者      : 吉本竜
 *  内容        : ランダム生成Roomの配置位置と出入口情報を管理する
 *  履歴        : 2026/04/27 新規作成(ヨシモト)
 *                2026/05/06 出入口用途と敵侵入設定の取得処理を追加(ヨシモト)
 *==================================================*/

/// <summary>
/// ルーム生成位置と、各方向の出入口情報を持つポイントです。
/// </summary>
[DisallowMultipleComponent]
public class CS_RoomCreatePoint : MonoBehaviour
{
    [Header("部屋のタイプ")]
    [SerializeField]
    private CSE_RoomTypeEnum e_RoomType;

    /// <summary>
    /// 部屋の種類
    /// </summary>
    public CSE_RoomTypeEnum RoomType
    {
        get { return e_RoomType; }
    }

    [Header("右出口の設定")]
    [SerializeField]
    private CS_RoomMoveConnection cs_RightConnection = new CS_RoomMoveConnection();

    [Header("左出口の設定")]
    [SerializeField]
    private CS_RoomMoveConnection cs_LeftConnection = new CS_RoomMoveConnection();

    [Header("前出口の設定")]
    [SerializeField]
    private CS_RoomMoveConnection cs_FrontConnection = new CS_RoomMoveConnection();

    [Header("後ろ出口の設定")]
    [SerializeField]
    private CS_RoomMoveConnection cs_BackConnection = new CS_RoomMoveConnection();

    /// <summary>
    /// 右のドアのワールド座標
    /// </summary>
    private Transform RightDoorPoints;

    /// <summary>
    /// 左のドアのワールド座標
    /// </summary>
    private Transform LeftDoorPoints;

    /// <summary>
    /// 前のドアのワールド座標
    /// </summary>
    private Transform FrontDoorPoints;

    /// <summary>
    /// 後ろのドアのワールド座標
    /// </summary>
    private Transform BackDoorPoints;

    /// <summary>
    /// ヒエログリフをまとめているオブジェクトのRoom内パス
    /// </summary>
    private string s_hieroglyph_path = "GameObject/Hieroglyphs";

    /// <summary>
    /// このRoomCreatePoint内にあるヒエログリフのオブジェクト
    /// </summary>
    private List<GameObject> gl_hieroglyph_obj = new List<GameObject>();

    /// <summary>
    /// ヒエログリフのタグ名
    /// </summary>
    private String s_target_hieroglyph_tag = "Hieroglyph";

    /// <summary>
    /// 指定方向のワープ接続情報を取得します。
    /// 敵出入口や未設定の場合はfalseを返します。
    /// </summary>
    /// <param name="e_FromDirection">このRoomから出る方向。</param>
    /// <param name="cs_Connection">取得した接続情報。</param>
    /// <returns>ワープ接続先がある場合はtrue。</returns>
    public bool TryGetConnection(
        CSE_RoomDoorDirection e_FromDirection,
        out CS_RoomMoveConnection cs_Connection)
    {
        cs_Connection = GetConnection(e_FromDirection);

        if (cs_Connection == null)
        {
            return false;
        }

        return cs_Connection.HasTarget;
    }

    /// <summary>
    /// 生成されたRoom内からHieroglyphタグの付いたオブジェクトを取得します。
    /// </summary>
    public void SetHieroglyphObjects()
    {
        gl_hieroglyph_obj.Clear();

        Transform tr_HieroglyphRoot = FindHieroglyphRoot();

        if (tr_HieroglyphRoot == null)
        {
            Debug.LogWarning(
                "[Hieroglyph取得失敗]"
                + "\nRoomCreatePoint : " + GetHierarchyPath(transform)
                + "\n検索パス : " + s_hieroglyph_path,
                this);

            return;
        }

        // 非アクティブのヒエログリフも含めて取得します。
        Transform[] tr_ChildTransforms =
            tr_HieroglyphRoot.GetComponentsInChildren<Transform>(true);

        for (int i = 0 ; i < tr_ChildTransforms.Length ; i++)
        {
            Transform tr_Target = tr_ChildTransforms[i];

            // Hieroglyphsをまとめている親自身は除外します。
            if (tr_Target == tr_HieroglyphRoot)
            {
                continue;
            }

            if (!tr_Target.CompareTag(s_target_hieroglyph_tag))
            {
                continue;
            }

            gl_hieroglyph_obj.Add(tr_Target.gameObject);
        }

        // 取得したヒエログリフへ、
        // 各扉方向に設定されたマテリアルを反映します。
        SetHieroglyphMaterials();

        //ShowHieroglyphDebugLog();
    }

    /// <summary>
    /// 各扉設定に応じて、同じ方向のヒエログリフへマテリアルを設定します。
    /// DoorUsageTypeがNoneの扉は処理しません。
    /// </summary>
    private void SetHieroglyphMaterials()
    {
        SetHieroglyphMaterialByConnection(cs_RightConnection);
        SetHieroglyphMaterialByConnection(cs_LeftConnection);
        SetHieroglyphMaterialByConnection(cs_FrontConnection);
        SetHieroglyphMaterialByConnection(cs_BackConnection);
    }

    /// <summary>
    /// 接続情報の方向と一致するヒエログリフへマテリアルを設定します。
    /// </summary>
    /// <param name="cs_Connection">確認する扉の接続情報。</param>
    private void SetHieroglyphMaterialByConnection(
        CS_RoomMoveConnection cs_Connection)
    {
        if (cs_Connection == null)
        {
            return;
        }

        // 使用しない扉にはヒエログリフ用マテリアルを設定しません。
        if (cs_Connection.GetDoorUsageType == CSE_RoomDoorUsageType.None)
        {
            return;
        }

        Material m_HieroglyphMaterial =
            cs_Connection.HierographMaterial;

        if (m_HieroglyphMaterial == null)
        {
            Debug.LogWarning(
                "[HieroglyphMaterial設定失敗]"
                + "\nRoomCreatePoint : " + name
                + "\n扉方向 : " + cs_Connection.DoorDirection
                + "\n差し替え用Materialが設定されていません。",
                this);

            return;
        }

        for (int i = 0 ; i < gl_hieroglyph_obj.Count ; i++)
        {
            GameObject go_Hieroglyph = gl_hieroglyph_obj[i];

            if (go_Hieroglyph == null)
            {
                continue;
            }

            CS_HieroglyphDirectionType cs_DirectionType =
                go_Hieroglyph.GetComponent<CS_HieroglyphDirectionType>();

            if (cs_DirectionType == null)
            {
                continue;
            }

            // 扉に設定された方向と
            // ヒエログリフ側の方向が一致するものだけ変更します。
            if (cs_DirectionType.GetDirection() !=
                cs_Connection.DoorDirection)
            {
                continue;
            }

            ReplaceAllRendererMaterials(
                go_Hieroglyph,
                m_HieroglyphMaterial);
        }
    }

    /// <summary>
    /// 対象オブジェクトとその子にある全RendererのMaterialを、
    /// 指定されたMaterial1つだけに置き換えます。
    /// </summary>
    /// <param name="go_Target">マテリアルを変更するヒエログリフ。</param>
    /// <param name="m_NewMaterial">新しく設定するマテリアル。</param>
    private void ReplaceAllRendererMaterials(
        GameObject go_Target,
        Material m_NewMaterial)
    {
        if (go_Target == null || m_NewMaterial == null)
        {
            return;
        }

        // ヒエログリフ本体だけでなく、
        // 子オブジェクトにRendererがある場合も全て取得します。
        Renderer[] rendererArray =
            go_Target.GetComponentsInChildren<Renderer>(true);

        for (int i = 0 ; i < rendererArray.Length ; i++)
        {
            Renderer targetRenderer = rendererArray[i];

            if (targetRenderer == null)
            {
                continue;
            }

            // 既存のMaterialスロットを全て外し、
            // 指定Material1つだけに置き換えます。
            targetRenderer.sharedMaterials =
                new Material[]
                {
                m_NewMaterial
                };
        }
    }

    /// <summary>
    /// s_hieroglyph_pathで指定されたヒエログリフの親を探します。
    /// </summary>
    /// <returns>見つかったヒエログリフの親Transform。</returns>
    private Transform FindHieroglyphRoot()
    {
        // RoomCreatePoint直下から検索します。
        Transform tr_HieroglyphRoot = transform.Find(s_hieroglyph_path);

        if (tr_HieroglyphRoot != null)
        {
            return tr_HieroglyphRoot;
        }

        // RoomCreatePoint直下に生成されたRoomがある場合は、
        // そのRoomを起点として指定パスを検索します。
        for (int i = 0 ; i < transform.childCount ; i++)
        {
            Transform tr_RoomChild = transform.GetChild(i);

            tr_HieroglyphRoot =
                tr_RoomChild.Find(s_hieroglyph_path);

            if (tr_HieroglyphRoot != null)
            {
                return tr_HieroglyphRoot;
            }
        }

        return null;
    }

    /// <summary>
    /// 取得したヒエログリフをConsoleへ表示します。
    /// </summary>
    //private void ShowHieroglyphDebugLog()
    //{
    //    string s_Log =
    //        "===== ヒエログリフ取得結果 ====="
    //        + "\nRoomCreatePoint : " + GetHierarchyPath(transform)
    //        + "\n取得数 : " + gl_hieroglyph_obj.Count;
    //
    //    for (int i = 0 ; i < gl_hieroglyph_obj.Count ; i++)
    //    {
    //        GameObject go_Hieroglyph = gl_hieroglyph_obj[i];
    //
    //        s_Log +=
    //            "\n[" + i + "] "
    //            + GetHierarchyPath(go_Hieroglyph.transform);
    //    }
    //
    //    Debug.Log(s_Log, this);
    //}

    /// <summary>
    /// 対象オブジェクトのHierarchy上のパスを取得します。
    /// </summary>
    /// <param name="tr_Target">パスを取得するTransform。</param>
    /// <returns>Hierarchy上のパス。</returns>
    private string GetHierarchyPath(Transform tr_Target)
    {
        if (tr_Target == null)
        {
            return "null";
        }

        string s_Path = tr_Target.name;
        Transform tr_Current = tr_Target.parent;

        while (tr_Current != null)
        {
            s_Path = tr_Current.name + "/" + s_Path;
            tr_Current = tr_Current.parent;
        }

        return s_Path;
    }

    /// <summary>
    /// 指定方向の敵出入口データを取得します。
    /// 敵出入口ではない場合、またはデータ未設定の場合はfalseを返します。
    /// </summary>
    /// <param name="e_FromDirection">確認したい出入口方向。</param>
    /// <param name="cs_EnemyEntryDataSO">取得した敵出入口データ。</param>
    /// <returns>敵出入口データがある場合はtrue。</returns>
    public bool TryGetEnemyEntryData(
        CSE_RoomDoorDirection e_FromDirection,
        out CSS_RoomEnemyEntryData cs_EnemyEntryDataSO)
    {
        cs_EnemyEntryDataSO = null;

        CS_RoomMoveConnection cs_Connection = GetConnection(e_FromDirection);

        if (cs_Connection == null)
        {
            return false;
        }

        if (!(cs_Connection.GetEnemyEntryCount() > 0))
        {
            return false;
        }

        cs_EnemyEntryDataSO = cs_Connection.RoomEnemyEntryDataSO;
        return true;
    }

    /// <summary>
    /// 指定方向の敵最大出現数を取得します。
    /// 敵出入口ではない場合は0を返します。
    /// </summary>
    /// <param name="e_FromDirection">確認したい出入口方向。</param>
    /// <returns>敵の最大出現数。</returns>
    public int GetMaxEnemySpawnCount(CSE_RoomDoorDirection e_FromDirection)
    {
        CS_RoomMoveConnection cs_Connection = GetConnection(e_FromDirection);

        if (cs_Connection == null)
        {
            return 0;
        }

        return cs_Connection.GetMaxEnemySpawnCount();
    }

    /// <summary>
    /// 指定方向の敵侵入数を取得します。
    /// 互換用として、最大出現数を返します。
    /// </summary>
    /// <param name="e_FromDirection">確認したい出入口方向。</param>
    /// <returns>敵の最大出現数。</returns>
    public int GetEnemyEntryCount(CSE_RoomDoorDirection e_FromDirection)
    {
        return GetMaxEnemySpawnCount(e_FromDirection);
    }

    /// <summary>
    /// 指定方向の扉用途を取得します。
    /// </summary>
    /// <param name="e_FromDirection">確認したい出入口方向。</param>
    /// <returns>扉の用途。</returns>
    public CSE_RoomDoorUsageType GetDoorUsageType(CSE_RoomDoorDirection e_FromDirection)
    {
        CS_RoomMoveConnection cs_Connection = GetConnection(e_FromDirection);

        if (cs_Connection == null)
        {
            return CSE_RoomDoorUsageType.None;
        }

        return cs_Connection.GetDoorUsageType;
    }

    /// <summary>
    /// ワープ接続先が設定されている方向を全て取得します。
    /// </summary>
    /// <returns>ワープ接続先がある方向リスト。</returns>
    public List<CSE_RoomDoorDirection> GetConnectDirections()
    {
        List<CSE_RoomDoorDirection> list_ConnectDirections = new List<CSE_RoomDoorDirection>();

        if (cs_RightConnection.HasTarget)
        {
            list_ConnectDirections.Add(CSE_RoomDoorDirection.Right);
        }

        if (cs_LeftConnection.HasTarget)
        {
            list_ConnectDirections.Add(CSE_RoomDoorDirection.Left);
        }

        if (cs_FrontConnection.HasTarget)
        {
            list_ConnectDirections.Add(CSE_RoomDoorDirection.Front);
        }

        if (cs_BackConnection.HasTarget)
        {
            list_ConnectDirections.Add(CSE_RoomDoorDirection.Back);
        }

        return list_ConnectDirections;
    }

    /// <summary>
    /// 敵出入口として設定されている方向を全て取得します。
    /// </summary>
    /// <returns>敵出入口の方向リスト。</returns>
    public List<CSE_RoomDoorDirection> GetEnemyEntryDirections()
    {
        List<CSE_RoomDoorDirection> list_EnemyEntryDirections = new List<CSE_RoomDoorDirection>();

        if (cs_RightConnection.GetEnemyEntryCount() > 0)
        {
            list_EnemyEntryDirections.Add(CSE_RoomDoorDirection.Right);
        }

        if (cs_LeftConnection.GetEnemyEntryCount() > 0)
        {
            list_EnemyEntryDirections.Add(CSE_RoomDoorDirection.Left);
        }

        if (cs_FrontConnection.GetEnemyEntryCount() > 0)
        {
            list_EnemyEntryDirections.Add(CSE_RoomDoorDirection.Front);
        }

        if (cs_BackConnection.GetEnemyEntryCount() > 0)
        {
            list_EnemyEntryDirections.Add(CSE_RoomDoorDirection.Back);
        }

        return list_EnemyEntryDirections;
    }

    /// <summary>
    /// 全ての敵出入口データをクリアします。
    /// </summary>
    public void ClearEnemyEntryDirections()
    {
        cs_RightConnection.ClearEnemyEntryData();
        cs_LeftConnection.ClearEnemyEntryData();
        cs_FrontConnection.ClearEnemyEntryData();
        cs_BackConnection.ClearEnemyEntryData();
    }

    /// <summary>
    /// 指定方向の接続情報を取得します。
    /// </summary>
    /// <param name="e_FromDirection">取得したい方向。</param>
    /// <returns>接続情報。</returns>
    public CS_RoomMoveConnection GetConnection(CSE_RoomDoorDirection e_FromDirection)
    {
        switch (e_FromDirection)
        {
            case CSE_RoomDoorDirection.Right:
                return cs_RightConnection;

            case CSE_RoomDoorDirection.Left:
                return cs_LeftConnection;

            case CSE_RoomDoorDirection.Front:
                return cs_FrontConnection;

            case CSE_RoomDoorDirection.Back:
                return cs_BackConnection;

            default:
                return null;
        }
    }

    /// <summary>
    /// 指定方向の敵出入口データを設定します。
    /// </summary>
    /// <param name="e_FromDirection">指定方向</param>
    /// <param name="newData">設定する敵出入口データ</param>
    public void SetEnemyData(CSE_RoomDoorDirection e_FromDirection, CSS_RoomEnemyEntryData newData)
    {
        CS_RoomMoveConnection cs_Connection = GetConnection(e_FromDirection);
        if (cs_Connection == null)
        {
            Debug.LogError($"指定された方向の接続情報が見つかりません。方向: {e_FromDirection}");
            return;
        }
        if (!cs_Connection.IsEnemyEntryDoor)
        {
            Debug.LogError($"指定された方向は敵出入口ではありません。方向: {e_FromDirection}");
            return;
        }
        cs_Connection.SetEnemyEntryData(newData);
    }

    /// <summary>
    /// 各ドアのワールド座標を設定します。
    /// </summary>
    private void SetDoorWorldPosition()
    {
        // 自分と子オブジェクトのCS_RoomMovePointコンポーネントを全て取得
        CS_RoomMovePoint[] components = GetComponentsInChildren<CS_RoomMovePoint>();

        // チェック用データ
        CSE_RoomDoorUsageType data = cs_RightConnection.GetDoorUsageType;

        // ドアがある場合のみ、ワールド座標を設定
        if (data != CSE_RoomDoorUsageType.None)
        {
            // 取得したコンポーネントの数だけループ
            for (int i = 0 ; i < components.Length ; i++)
            {
                // 右のドア座標を設定
                if (components[i].MoveDirection == CSE_RoomDoorDirection.Right)
                {
                    // ワールド座標を設定
                    RightDoorPoints = components[i].transform;
                }
            }
        }

        // 左のドア座標を設定
        data = cs_LeftConnection.GetDoorUsageType;
        if (data != CSE_RoomDoorUsageType.None)
        {
            for (int i = 0 ; i < components.Length ; i++)
            {
                if (components[i].MoveDirection == CSE_RoomDoorDirection.Left)
                {
                    LeftDoorPoints = components[i].transform;
                }
            }
        }

        // 前のドア座標を設定
        data = cs_FrontConnection.GetDoorUsageType;
        if (data != CSE_RoomDoorUsageType.None)
        {
            for (int i = 0 ; i < components.Length ; i++)
            {
                if (components[i].MoveDirection == CSE_RoomDoorDirection.Front)
                {
                    FrontDoorPoints = components[i].transform;
                }
            }
        }

        // 後ろのドア座標を設定
        data = cs_BackConnection.GetDoorUsageType;
        if (data != CSE_RoomDoorUsageType.None)
        {
            for (int i = 0 ; i < components.Length ; i++)
            {
                if (components[i].MoveDirection == CSE_RoomDoorDirection.Back)
                {
                    BackDoorPoints = components[i].transform;
                }
            }
        }
    }

    /// <summary>
    /// ドアのワールド座標を取得します。(RoomCreatePointをゲットコンポオ―ネントする前提)
    /// </summary>
    /// <param name="data">取得したい方向</param>
    /// <returns></returns>
    public Transform GetRoomDoorPosition(CSE_RoomDoorDirection data)
    {
        SetDoorWorldPosition();

        switch (data)
        {
        case CSE_RoomDoorDirection.Right:
            return RightDoorPoints;
        case CSE_RoomDoorDirection.Left:
            return LeftDoorPoints;
        case CSE_RoomDoorDirection.Front:
            return FrontDoorPoints;
        case CSE_RoomDoorDirection.Back:
            return BackDoorPoints;
        default:
            return null;
        }
    }
}
