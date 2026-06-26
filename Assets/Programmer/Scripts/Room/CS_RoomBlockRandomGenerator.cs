using System.Collections;
using UnityEngine;

/*==================================================
 *  ファイル名  : CS_RoomBlockRandomGenerator.cs
 *  制作者      : 吉本竜
 *  内容        : Play中のランダムRoom生成・再生成を管理するクラス
 *  履歴        : 2026/05/25 CS_RoomBlockPrefabGeneratorから実行時ランダム生成処理を分離(ヨシモト)
 *==================================================*/

/// <summary>
/// Play中のランダムRoom生成を管理するクラスです。
/// </summary>
[DisallowMultipleComponent]
public class CS_RoomBlockRandomGenerator : MonoBehaviour
{
    [Header("Room生成本体")]
    [SerializeField]
    private CS_RoomBlockPrefabGenerator cs_RoomBlockPrefabGenerator;

    [Header("実行時ランダム生成設定")]
    [SerializeField]
    private bool bool_IsAutoRegenerateRandomOnStart = true;

    [Header("ワープのマネージャーからの取得")]
    [SerializeField]
    private CS_WarpSpawn cs_WarpSpawn;

    private bool bool_IsRuntimeRegenerating = false;

    /// <summary>
    /// 部屋生成完了フラグ
    /// </summary>
    public bool b_IsRuntimeRegenerating {get; private set;}

    /// <summary>
    /// Reset時に同じGameObjectからRoom生成本体を取得します。
    /// </summary>
    private void Reset()
    {
        cs_RoomBlockPrefabGenerator = GetComponent<CS_RoomBlockPrefabGenerator>();
    }

    /// <summary>
    /// Play開始時にRandom設定のRoomだけを自動再生成します。
    /// </summary>
    private void Awake()
    {
        b_IsRuntimeRegenerating = false;

        if (!Application.isPlaying)
        {
            return;
        }

        if (!TryGetGenerator())
        {
            return;
        }

        if (!bool_IsAutoRegenerateRandomOnStart)
        {
            cs_RoomBlockPrefabGenerator.RebuildGeneratedRoomLinks();
            cs_RoomBlockPrefabGenerator.CreatePlayerAtFirstRoomStartPoint();
            return;
        }

        StartCoroutine(RegenerateRandomRoomBlocksRuntimeCoroutine());
    }

    /// <summary>
    /// Random設定のRoomを生成します。
    /// </summary>
    [ContextMenu("ランダムルームブロックを生成")]
    public void GenerateRandomRoomBlocks()
    {
        if (!TryGetGenerator())
        {
            return;
        }

        cs_RoomBlockPrefabGenerator.GenerateRoomBlocksByType(
            CSE_RoomBlockGenerateType.Random,
            false);
    }

    /// <summary>
    /// Random設定のRoomを削除してから再生成します。
    /// </summary>
    [ContextMenu("ランダムルームブロックを再生成")]
    public void RegenerateRandomRoomBlocks()
    {
        if (!TryGetGenerator())
        {
            return;
        }

        if (Application.isPlaying)
        {
            StartCoroutine(RegenerateRandomRoomBlocksRuntimeCoroutine());
            return;
        }

        cs_RoomBlockPrefabGenerator.DeleteGeneratedRoomBlocksByType(CSE_RoomBlockGenerateType.Random);

        cs_RoomBlockPrefabGenerator.GenerateRoomBlocksByType(
            CSE_RoomBlockGenerateType.Random,
            true);
    }

    /// <summary>
    /// Play中用のRandom Room再生成処理です。生成ごワープの生成も行います。
    /// </summary>
    private IEnumerator RegenerateRandomRoomBlocksRuntimeCoroutine()
    {
        if (bool_IsRuntimeRegenerating)
        {
            yield break;
        }

        bool_IsRuntimeRegenerating = true;

        cs_RoomBlockPrefabGenerator.DeleteGeneratedRoomBlocksByType(CSE_RoomBlockGenerateType.Random);

        yield return null;

        cs_RoomBlockPrefabGenerator.GenerateRoomBlocksByType(
            CSE_RoomBlockGenerateType.Random,
            true);

        yield return null;

        // 部屋生成完了フラグをtrueに設定します。
        b_IsRuntimeRegenerating = true;

        // ワープ生成はRoom生成後に行う必要があるため、ここで実行します。
        if (cs_WarpSpawn != null)
        {
            cs_WarpSpawn.WarpPointStart();
            cs_WarpSpawn.SpawnWarp();
        }

        cs_RoomBlockPrefabGenerator.CreatePlayerAtFirstRoomStartPoint();

        bool_IsRuntimeRegenerating = false;
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
            Debug.LogWarning("[RoomBlockRandomGenerator] CS_RoomBlockPrefabGeneratorが設定されていません。");
            return false;
        }

        return true;
    }
}
