/*
+=====================================
 ファイル名 : CSED_CreateTools_GeneratedEditorRecord.cs
 概要     : CreateToolsで生成したEditor情報を保持するデータ
 作者     : ヨシモト リョウ
 履歴     : 2026/05/19 新規作成
            2026/05/20 Load用の復元データを追加
=====================================+
*/

#if UNITY_EDITOR
using System;
using System.Collections.Generic;

/// <summary>
/// CreateToolsで生成したEditor情報を保持するデータです。
/// </summary>
[Serializable]
public class CSED_CreateTools_GeneratedEditorRecord
{
    /// <summary>
    /// 表示タイトルです。
    /// </summary>
    public string titleName;

    /// <summary>
    /// 制作者名です。
    /// </summary>
    public string authorName;

    /// <summary>
    /// 作成日時です。
    /// </summary>
    public string createdDate;

    /// <summary>
    /// 更新日時です。
    /// </summary>
    public string updatedDate;

    /// <summary>
    /// EditorWindowクラス名です。
    /// </summary>
    public string editorClassName;

    /// <summary>
    /// ScriptableObjectデータクラス名です。
    /// </summary>
    public string dataClassName;

    /// <summary>
    /// メニューパスです。
    /// </summary>
    public string menuPath;

    /// <summary>
    /// EditorWindowスクリプトパスです。
    /// </summary>
    public string editorScriptPath;

    /// <summary>
    /// ScriptableObjectスクリプトパスです。
    /// </summary>
    public string dataScriptPath;

    /// <summary>
    /// 履歴日付です。
    /// </summary>
    public string headerHistoryDate;

    /// <summary>
    /// EditorWindow側の概要です。
    /// </summary>
    public string editorHeaderContents;

    /// <summary>
    /// ScriptableObject側の概要です。
    /// </summary>
    public string dataHeaderContents;

    /// <summary>
    /// デフォルトAsset名です。
    /// </summary>
    public string defaultAssetName;

    /// <summary>
    /// デフォルトAsset保存先です。
    /// </summary>
    public string defaultAssetFolder;

    /// <summary>
    /// 復元用FieldData一覧です。
    /// </summary>
    public List<CSED_CreateTools_FieldDataSaveData> fieldSaveDataList =
        new List<CSED_CreateTools_FieldDataSaveData>();
}
#endif
