using UnityEngine;
using System.Collections.Generic;

/*==================================================
 *  ファイル名  : CSS_MapData.cs
 *  制作者      : 吉本竜
 *  内容        : マップデータを保持する ScriptableObject
 *                床・壁・ドア・固定モデルの情報を保持し、
 *                キー名から対象データを取得できるようにする
 *  履歴        : 2026/04/24 新規作成(ヨシモト)
 *                2026/04/24 ドア・モデルをキー検索できる構造に変更
 *==================================================*/

/// <summary>
/// マップデータを保持する ScriptableObject です。
/// キー文字列を使って、ドアや固定モデルの情報を取得できます。
/// </summary>
[CreateAssetMenu(menuName = "MapData_Scriptable")]
public class CSS_MapData : ScriptableObject
{
    /// <summary>
    /// ドア情報をまとめて保持する内部データです。
    /// </summary>
    [System.Serializable]
    private class CS_DoorData
    {
        [Header("ドアのキー名"), SerializeField]
        private string s_DoorKey;

        [Header("ドアオブジェクト"), SerializeField]
        private GameObject go_DoorObject;

        [Header("ドアの配置座標"), SerializeField]
        private Vector2Int v2n_DoorBlockPos;

        /// <summary>
        /// ドアのキー名を取得します。
        /// </summary>
        public string DoorKey => s_DoorKey;

        /// <summary>
        /// ドアオブジェクトを取得します。
        /// </summary>
        public GameObject DoorObject => go_DoorObject;

        /// <summary>
        /// ドアの配置座標を取得します。
        /// </summary>
        public Vector2Int DoorBlockPos => v2n_DoorBlockPos;
    }

    /// <summary>
    /// 固定モデル情報をまとめて保持する内部データです。
    /// </summary>
    [System.Serializable]
    private class CS_ModelData
    {
        [Header("モデルのキー名"), SerializeField]
        private string s_ModelKey;

        [Header("固定で出すモデル"), SerializeField]
        private GameObject go_ModelObject;

        [Header("モデルの配置座標"), SerializeField]
        private Vector2Int v2n_ModelBlockPos;

        /// <summary>
        /// モデルのキー名を取得します。
        /// </summary>
        public string ModelKey => s_ModelKey;

        /// <summary>
        /// モデルオブジェクトを取得します。
        /// </summary>
        public GameObject ModelObject => go_ModelObject;

        /// <summary>
        /// モデルの配置座標を取得します。
        /// </summary>
        public Vector2Int ModelBlockPos => v2n_ModelBlockPos;
    }

    [Header("床ブロック"), SerializeField]
    private GameObject go_FloorBlock;

    [Header("壁ブロック"), SerializeField]
    private GameObject go_WallBlock;

    [Header("ドア一覧"), SerializeField]
    private List<CS_DoorData> lcs_DoorData = new List<CS_DoorData>();

    [Header("固定モデル一覧"), SerializeField]
    private List<CS_ModelData> lcs_ModelData = new List<CS_ModelData>();

    [Header("マップの縦大きさ"), SerializeField]
    private int n_MapLength;

    [Header("マップの横大きさ"), SerializeField]
    private int n_MapWidth;

    private Dictionary<string, CS_DoorData> dic_DoorData;
    private Dictionary<string, CS_ModelData> dic_ModelData;
    private bool b_IsInitialized = false;

    /// <summary>
    /// 床ブロックを取得します。
    /// </summary>
    public GameObject FloorBlock => go_FloorBlock;

    /// <summary>
    /// 壁ブロックを取得します。
    /// </summary>
    public GameObject WallBlock => go_WallBlock;

    /// <summary>
    /// マップの縦サイズを取得します。
    /// </summary>
    public int MapLength => n_MapLength;

    /// <summary>
    /// マップの横サイズを取得します。
    /// </summary>
    public int MapWidth => n_MapWidth;

    /// <summary>
    /// ScriptableObject 有効化時に検索辞書を初期化します。
    /// </summary>
    private void OnEnable()
    {
        InitializeLookup();
    }

#if UNITY_EDITOR
    /// <summary>
    /// Inspector の値変更時に検索辞書を再構築します。
    /// </summary>
    private void OnValidate()
    {
        InitializeLookup();
    }
#endif

    /// <summary>
    /// ドア・モデル検索用の辞書を初期化します。
    /// </summary>
    private void InitializeLookup()
    {
        dic_DoorData = new Dictionary<string, CS_DoorData>();
        dic_ModelData = new Dictionary<string, CS_ModelData>();

        for (int i = 0 ; i < lcs_DoorData.Count ; i++)
        {
            if (lcs_DoorData[i] == null)
            {
                continue;
            }

            string key = lcs_DoorData[i].DoorKey;

            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning($"[CSS_MapData] DoorData のキーが空です。Index : {i}", this);
                continue;
            }

            if (dic_DoorData.ContainsKey(key))
            {
                Debug.LogWarning($"[CSS_MapData] DoorData のキーが重複しています。Key : {key}", this);
                continue;
            }

            dic_DoorData.Add(key, lcs_DoorData[i]);
        }

        for (int i = 0 ; i < lcs_ModelData.Count ; i++)
        {
            if (lcs_ModelData[i] == null)
            {
                continue;
            }

            string key = lcs_ModelData[i].ModelKey;

            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning($"[CSS_MapData] ModelData のキーが空です。Index : {i}", this);
                continue;
            }

            if (dic_ModelData.ContainsKey(key))
            {
                Debug.LogWarning($"[CSS_MapData] ModelData のキーが重複しています。Key : {key}", this);
                continue;
            }

            dic_ModelData.Add(key, lcs_ModelData[i]);
        }

        b_IsInitialized = true;
    }

    /// <summary>
    /// 検索辞書が未初期化の場合に初期化します。
    /// </summary>
    private void EnsureInitialized()
    {
        if (b_IsInitialized == false)
        {
            InitializeLookup();
        }
    }

    /// <summary>
    /// キー名からドアオブジェクトを取得します。
    /// </summary>
    /// <param name="s_DoorKey">取得したいドアのキー名</param>
    /// <returns>一致したドアオブジェクト。存在しない場合は null</returns>
    public GameObject GetDoorObject(string s_DoorKey)
    {
        EnsureInitialized();

        if (dic_DoorData.TryGetValue(s_DoorKey, out CS_DoorData doorData))
        {
            return doorData.DoorObject;
        }

        Debug.LogWarning($"[CSS_MapData] DoorKey が見つかりません。Key : {s_DoorKey}", this);
        return null;
    }

    /// <summary>
    /// キー名からドアの配置座標を取得します。
    /// </summary>
    /// <param name="s_DoorKey">取得したいドアのキー名</param>
    /// <returns>一致したドアの配置座標。存在しない場合は Vector2Int.zero</returns>
    public Vector2Int GetDoorBlockPos(string s_DoorKey)
    {
        EnsureInitialized();

        if (dic_DoorData.TryGetValue(s_DoorKey, out CS_DoorData doorData))
        {
            return doorData.DoorBlockPos;
        }

        Debug.LogWarning($"[CSS_MapData] DoorKey が見つかりません。Key : {s_DoorKey}", this);
        return Vector2Int.zero;
    }

    /// <summary>
    /// キー名からモデルオブジェクトを取得します。
    /// </summary>
    /// <param name="s_ModelKey">取得したいモデルのキー名</param>
    /// <returns>一致したモデルオブジェクト。存在しない場合は null</returns>
    public GameObject GetModelObject(string s_ModelKey)
    {
        EnsureInitialized();

        if (dic_ModelData.TryGetValue(s_ModelKey, out CS_ModelData modelData))
        {
            return modelData.ModelObject;
        }

        Debug.LogWarning($"[CSS_MapData] ModelKey が見つかりません。Key : {s_ModelKey}", this);
        return null;
    }

    /// <summary>
    /// キー名からモデルの配置座標を取得します。
    /// </summary>
    /// <param name="s_ModelKey">取得したいモデルのキー名</param>
    /// <returns>一致したモデルの配置座標。存在しない場合は Vector2Int.zero</returns>
    public Vector2Int GetModelBlockPos(string s_ModelKey)
    {
        EnsureInitialized();

        if (dic_ModelData.TryGetValue(s_ModelKey, out CS_ModelData modelData))
        {
            return modelData.ModelBlockPos;
        }

        Debug.LogWarning($"[CSS_MapData] ModelKey が見つかりません。Key : {s_ModelKey}", this);
        return Vector2Int.zero;
    }

    /// <summary>
    /// キー名からドア情報の取得を試みます。
    /// </summary>
    /// <param name="s_DoorKey">取得したいドアのキー名</param>
    /// <param name="go_DoorObject">取得したドアオブジェクト</param>
    /// <param name="v2n_BlockPos">取得したドア配置座標</param>
    /// <returns>取得に成功した場合は true</returns>
    public bool TryGetDoorData(string s_DoorKey, out GameObject go_DoorObject, out Vector2Int v2n_BlockPos)
    {
        EnsureInitialized();

        if (dic_DoorData.TryGetValue(s_DoorKey, out CS_DoorData doorData))
        {
            go_DoorObject = doorData.DoorObject;
            v2n_BlockPos = doorData.DoorBlockPos;
            return true;
        }

        go_DoorObject = null;
        v2n_BlockPos = Vector2Int.zero;
        return false;
    }

    /// <summary>
    /// キー名からモデル情報の取得を試みます。
    /// </summary>
    /// <param name="s_ModelKey">取得したいモデルのキー名</param>
    /// <param name="go_ModelObject">取得したモデルオブジェクト</param>
    /// <param name="v2n_BlockPos">取得したモデル配置座標</param>
    /// <returns>取得に成功した場合は true</returns>
    public bool TryGetModelData(string s_ModelKey, out GameObject go_ModelObject, out Vector2Int v2n_BlockPos)
    {
        EnsureInitialized();

        if (dic_ModelData.TryGetValue(s_ModelKey, out CS_ModelData modelData))
        {
            go_ModelObject = modelData.ModelObject;
            v2n_BlockPos = modelData.ModelBlockPos;
            return true;
        }

        go_ModelObject = null;
        v2n_BlockPos = Vector2Int.zero;
        return false;
    }
}
