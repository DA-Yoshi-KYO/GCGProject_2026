/*
+=====================================
 ファイル名 : CSS_CreateToolsGeneratedEditorList.cs
 概要     : CreateToolsで生成したEditor一覧を保存するScriptableObject
 作者     : ヨシモト リョウ
 履歴     : 2026/05/19 新規作成
=====================================+
*/

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CreateToolsで生成したEditor一覧を保存するScriptableObjectです。
/// </summary>
public class CSS_CreateToolsGeneratedEditorList : ScriptableObject
{
    /// <summary>
    /// 生成済みEditor一覧です。
    /// </summary>
    public List<CSED_CreateTools_GeneratedEditorRecord> generatedEditorRecordList =
        new List<CSED_CreateTools_GeneratedEditorRecord>();
}
#endif
