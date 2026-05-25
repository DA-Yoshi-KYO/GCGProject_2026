using UnityEngine;

/*==================================================
 *  ファイル名  : CS_RoomBlockEditorGenerator.cs
 *  制作者      : 吉本竜
 *  内容        : エディター上での固定Room生成・再生成・削除・接続更新を管理するクラス
 *  履歴        : 2026/05/25 CS_RoomBlockPrefabGeneratorからエディター生成操作を分離(ヨシモト)
 *==================================================*/

/// <summary>
/// エディター上でのRoom生成操作を管理するクラスです。
/// </summary>
[DisallowMultipleComponent]
public class CS_RoomBlockEditorGenerator : MonoBehaviour
{
    [Header("Room生成本体")]
    [SerializeField]
    private CS_RoomBlockPrefabGenerator cs_RoomBlockPrefabGenerator;

    /// <summary>
    /// Reset時に同じGameObjectからRoom生成本体を取得します。
    /// </summary>
    private void Reset()
    {
        cs_RoomBlockPrefabGenerator = GetComponent<CS_RoomBlockPrefabGenerator>();
    }

    /// <summary>
    /// Fixed設定のRoomを生成します。
    /// </summary>
    [ContextMenu("固定ルームブロックを生成")]
    public void GenerateFixedRoomBlocks()
    {
        if (!TryGetGenerator())
        {
            return;
        }

        if (Application.isPlaying)
        {
            Debug.LogWarning("[RoomBlockEditorGenerator] Play中は固定Roomのエディター生成を実行できません。");
            return;
        }

        cs_RoomBlockPrefabGenerator.GenerateRoomBlocksByType(
            CSE_RoomBlockGenerateType.Fixed,
            false);
    }

    /// <summary>
    /// Fixed設定のRoomを削除してから再生成します。
    /// </summary>
    [ContextMenu("固定ルームブロックを再生成")]
    public void RegenerateFixedRoomBlocks()
    {
        if (!TryGetGenerator())
        {
            return;
        }

        if (Application.isPlaying)
        {
            Debug.LogWarning("[RoomBlockEditorGenerator] Play中は固定Roomのエディター再生成を実行できません。");
            return;
        }

        cs_RoomBlockPrefabGenerator.DeleteGeneratedRoomBlocksByType(CSE_RoomBlockGenerateType.Fixed);

        cs_RoomBlockPrefabGenerator.GenerateRoomBlocksByType(
            CSE_RoomBlockGenerateType.Fixed,
            true);
    }

    /// <summary>
    /// 生成済みRoomをすべて削除します。
    /// </summary>
    [ContextMenu("生成済みルームブロックをすべて削除")]
    public void DeleteAllGeneratedRoomBlocks()
    {
        if (!TryGetGenerator())
        {
            return;
        }

        cs_RoomBlockPrefabGenerator.DeleteGeneratedRoomBlocksByType(CSE_RoomBlockGenerateType.Fixed);
        cs_RoomBlockPrefabGenerator.DeleteGeneratedRoomBlocksByType(CSE_RoomBlockGenerateType.Random);
        cs_RoomBlockPrefabGenerator.DeleteOldGeneratedRoot();

        Debug.Log("[RoomBlockEditorGenerator] 生成済みRoomをすべて削除しました。");
    }

    /// <summary>
    /// 生成済みRoomの接続を更新します。
    /// </summary>
    [ContextMenu("生成済みルーム接続を更新")]
    public void RebuildGeneratedRoomLinks()
    {
        if (!TryGetGenerator())
        {
            return;
        }

        cs_RoomBlockPrefabGenerator.RebuildGeneratedRoomLinks();
    }

    /// <summary>
    /// 外部からFixed Room生成を実行します。
    /// </summary>
    public void CreateRooms()
    {
        GenerateFixedRoomBlocks();
    }

    /// <summary>
    /// 外部からFixed Room再生成を実行します。
    /// </summary>
    public void RecreateRooms()
    {
        RegenerateFixedRoomBlocks();
    }

    /// <summary>
    /// 外部からRoom削除を実行します。
    /// </summary>
    public void DeleteRooms()
    {
        DeleteAllGeneratedRoomBlocks();
    }

    /// <summary>
    /// Room生成本体を取得できるか確認します。
    /// </summary>
    /// <returns>取得できる場合はtrue。</returns>
    private bool TryGetGenerator()
    {
        if (cs_RoomBlockPrefabGenerator == null)
        {
            cs_RoomBlockPrefabGenerator = GetComponent<CS_RoomBlockPrefabGenerator>();
        }

        if (cs_RoomBlockPrefabGenerator == null)
        {
            Debug.LogWarning("[RoomBlockEditorGenerator] CS_RoomBlockPrefabGeneratorが設定されていません。");
            return false;
        }

        return true;
    }
}
