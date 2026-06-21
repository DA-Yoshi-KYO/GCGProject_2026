/*
+=====================================
 ファイル名 : CSS_NewToolData.cs
 概要     : CreateToolsから自動生成されたScriptableObjectデータ
 作者     : ヨシモト リョウ
 履歴     : 2026/06/21 CreateToolsから自動生成
=====================================+
*/

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CSS_NewToolData", menuName = "Scriptable Objects/CSS_NewToolData")]
public class CSS_NewToolData : ScriptableObject
{
    /// <summary>
    /// NewIntField01です。
    /// </summary>
    [field: SerializeField]
    [field: Tooltip("n_Test")]
    public int NewIntField01 { get; set; }

    /// <summary>
    /// N_Test_Notです。
    /// </summary>
    [field: SerializeField]
    public int N_Test_Not { get; private set; }

    /// <summary>
    /// CreateToolsから値を初期化します。
    /// </summary>
#if UNITY_EDITOR
    public void InitializeFromCreateTools(
        int f_newIntField01,
        int f_n_Test_Not
        )
    {
        NewIntField01 = f_newIntField01;
        N_Test_Not = f_n_Test_Not;
    }
#endif

}
