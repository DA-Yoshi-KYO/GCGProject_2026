/*
+=====================================
 ファイル名 : CSED_CreateTools_GeneratedEditorRecord.cs
 概要     : CreateToolsで生成したEditor情報を保持するデータ
 作者     : ヨシモト リョウ
 履歴     : 2026/05/19 新規作成
=====================================+
*/

#if UNITY_EDITOR
using System;

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
    /// 作成日時です。
    /// </summary>
    public string createdDate;
}
#endif
