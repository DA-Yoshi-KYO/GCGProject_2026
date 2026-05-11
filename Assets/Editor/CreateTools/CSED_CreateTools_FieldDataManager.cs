/*
+=====================================
 ファイル名 : CSED_CreateTools_FieldDataManager.cs
 概要     : CreateToolsで使用する変数データ管理クラス
 作者     : ヨシモト リョウ
 履歴     : 2026/05/08 新規作成
            2026/05/08 FieldLayoutTypeの初期設定処理を追加
=====================================+
*/

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CreateToolsの変数データ管理処理をまとめるクラスです。
/// </summary>
public partial class CSED_CreateTools
{
    /// <summary>
    /// 変数データリストが使用可能な状態か確認します。
    /// </summary>
    private void EnsureFieldDataList()
    {
        if (m_FieldDataList == null)
        {
            m_FieldDataList = new List<CSED_CreateTools_FieldData>();
        }
    }

    /// <summary>
    /// 中央エリアに変数データを追加します。
    /// </summary>
    /// <param name="f_fieldType">追加する変数の型</param>
    private void AddFieldData(CSE_CreateTools_FieldType f_fieldType)
    {
        EnsureFieldDataList();

        string fieldName = CreateDefaultFieldName(f_fieldType);
        CSE_CreateTools_FieldLayoutType fieldLayoutType = CreateDefaultFieldLayoutType(f_fieldType);

        CSED_CreateTools_FieldData fieldData = new CSED_CreateTools_FieldData(
            f_fieldType,
            fieldName,
            fieldLayoutType);

        m_FieldDataList.Add(fieldData);

        m_SelectedFieldDataIndex = m_FieldDataList.Count - 1;

        if (m_FieldDataReorderableList != null)
        {
            m_FieldDataReorderableList.index = m_SelectedFieldDataIndex;
        }

        Debug.Log(
            "[CreateTools] 中央エリアに変数を追加しました : Type = "
            + GetFieldTypeDisplayName(f_fieldType)
            + " / Name = "
            + fieldName
            + " / Layout = "
            + fieldLayoutType);

        Repaint();
    }

    /// <summary>
    /// 変数型に応じた初期変数名を作成します。
    /// </summary>
    /// <param name="f_fieldType">変数の型</param>
    /// <returns>初期変数名</returns>
    private string CreateDefaultFieldName(CSE_CreateTools_FieldType f_fieldType)
    {
        int number = m_FieldDataList.Count + 1;

        switch (f_fieldType)
        {
            case CSE_CreateTools_FieldType.Int:
                return "newIntField" + number.ToString("00");

            case CSE_CreateTools_FieldType.Float:
                return "newFloatField" + number.ToString("00");

            case CSE_CreateTools_FieldType.String:
                return "newStringField" + number.ToString("00");

            case CSE_CreateTools_FieldType.Bool:
                return "newBoolField" + number.ToString("00");

            case CSE_CreateTools_FieldType.List:
                return "newListField" + number.ToString("00");

            default:
                return "newField" + number.ToString("00");
        }
    }

    /// <summary>
    /// 変数型に応じた初期表示レイアウトを取得します。
    /// </summary>
    /// <param name="f_fieldType">変数の型</param>
    /// <returns>初期表示レイアウト</returns>
    private CSED_CreateTools_FieldLayoutType CreateDefaultFieldLayoutType(CSE_CreateTools_FieldType f_fieldType)
    {
        switch (f_fieldType)
        {
            case CSE_CreateTools_FieldType.Int:
                return CSED_CreateTools_FieldLayoutType.InputField;

            case CSE_CreateTools_FieldType.Float:
                return CSED_CreateTools_FieldLayoutType.InputField;

            case CSE_CreateTools_FieldType.String:
                return CSED_CreateTools_FieldLayoutType.InputField;

            case CSE_CreateTools_FieldType.Bool:
                return CSED_CreateTools_FieldLayoutType.Toggle;

            case CSE_CreateTools_FieldType.List:
                return CSED_CreateTools_FieldLayoutType.ReorderableList;

            default:
                return CSED_CreateTools_FieldLayoutType.InputField;
        }
    }

    /// <summary>
    /// 選択中Fieldデータを取得します。
    /// </summary>
    /// <param name="f_fieldData">取得したFieldデータ</param>
    /// <returns>取得できた場合はtrue</returns>
    private bool TryGetSelectedFieldData(out CSED_CreateTools_FieldData f_fieldData)
    {
        EnsureFieldDataList();

        ClampSelectedFieldDataIndex();

        if (m_SelectedFieldDataIndex < 0 || m_SelectedFieldDataIndex >= m_FieldDataList.Count)
        {
            f_fieldData = null;
            return false;
        }

        f_fieldData = m_FieldDataList[m_SelectedFieldDataIndex];
        return true;
    }

    /// <summary>
    /// 選択中Field番号を設定します。
    /// </summary>
    /// <param name="f_index">選択するField番号</param>
    private void SetSelectedFieldDataIndex(int f_index)
    {
        EnsureFieldDataList();

        if (f_index < 0 || f_index >= m_FieldDataList.Count)
        {
            m_SelectedFieldDataIndex = -1;
        }
        else
        {
            m_SelectedFieldDataIndex = f_index;
        }

        Repaint();
    }

    /// <summary>
    /// 選択中Field番号を有効範囲に収めます。
    /// </summary>
    private void ClampSelectedFieldDataIndex()
    {
        if (m_FieldDataList == null || m_FieldDataList.Count <= 0)
        {
            m_SelectedFieldDataIndex = -1;
            return;
        }

        if (m_SelectedFieldDataIndex >= m_FieldDataList.Count)
        {
            m_SelectedFieldDataIndex = m_FieldDataList.Count - 1;
        }
    }
}
#endif
