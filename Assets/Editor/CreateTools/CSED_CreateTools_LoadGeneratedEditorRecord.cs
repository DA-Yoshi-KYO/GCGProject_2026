/*
+=====================================
 ファイル名 : CSED_CreateTools_LoadGeneratedEditorRecord.cs
 概要     : 生成済みEditor情報をCreateToolsへ復元する処理
 作者     : ヨシモト リョウ
 履歴     : 2026/05/20 新規作成
=====================================+
*/

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 生成済みEditor情報をCreateToolsへ復元する処理です。
/// </summary>
public partial class CSED_CreateTools
{
    /// <summary>
    /// 生成済みEditor情報をCreateToolsに読み込みます。
    /// </summary>
    /// <param name="f_record">読み込む生成済みEditor情報</param>
    public static void OpenAndLoadGeneratedEditorRecord(CSED_CreateTools_GeneratedEditorRecord f_record)
    {
        CSED_CreateTools createToolsWindow =
            GetWindow<CSED_CreateTools>("CreateTools");

        createToolsWindow.minSize = new Vector2(
            c_WindowMinWidth,
            c_WindowMinHeight);

        CSED_CreateTools_GeneratedEditorsWindow.OpenWindow();

        createToolsWindow.LoadGeneratedEditorRecord(f_record);
        createToolsWindow.Focus();
    }

    /// <summary>
    /// 生成済みEditor情報を現在のCreateTools設定へ反映します。
    /// </summary>
    /// <param name="f_record">読み込む生成済みEditor情報</param>
    private void LoadGeneratedEditorRecord(CSED_CreateTools_GeneratedEditorRecord f_record)
    {
        if (f_record == null)
        {
            return;
        }

        m_PreviewEditorTitleName = f_record.titleName;
        m_GeneratedToolWindowTitle = f_record.titleName;

        m_GeneratedHeaderAuthorName = GetLoadText(f_record.authorName, m_GeneratedHeaderAuthorName);
        m_GeneratedHeaderHistoryDate = GetLoadText(f_record.headerHistoryDate, m_GeneratedHeaderHistoryDate);
        m_GeneratedEditorHeaderContents = GetLoadText(f_record.editorHeaderContents, m_GeneratedEditorHeaderContents);
        m_GeneratedDataHeaderContents = GetLoadText(f_record.dataHeaderContents, m_GeneratedDataHeaderContents);

        m_GeneratedToolClassName = GetLoadText(f_record.editorClassName, m_GeneratedToolClassName);
        m_GeneratedScriptableObjectClassName = GetLoadText(f_record.dataClassName, m_GeneratedScriptableObjectClassName);
        m_GeneratedToolMenuPath = GetLoadText(f_record.menuPath, m_GeneratedToolMenuPath);

        m_GeneratedToolOutputFolderPath = GetLoadText(
           f_record.editorScriptFolderPath,
           GetFolderPathFromAssetPath(
              f_record.editorScriptPath,
              m_GeneratedToolOutputFolderPath));

        m_GeneratedScriptableObjectOutputFolderPath = GetLoadText(
            f_record.dataScriptFolderPath,
            GetFolderPathFromAssetPath(
                f_record.dataScriptPath,
                m_GeneratedScriptableObjectOutputFolderPath));

        m_GeneratedAssetOutputFolderPath = GetLoadText(
            f_record.assetSaveFolderPath,
            m_GeneratedAssetOutputFolderPath);

        m_GeneratedDefaultAssetName = GetLoadText(f_record.defaultAssetName, m_GeneratedDefaultAssetName);
        m_GeneratedDefaultAssetFolder = GetLoadText(f_record.defaultAssetFolder, m_GeneratedDefaultAssetFolder);

        m_FieldDataList = CreateFieldDataListFromSaveDataList(f_record.fieldSaveDataList);

        m_SelectedFieldDataIndex = -1;
        m_FieldDataReorderableList = null;

        Repaint();
    }

    /// <summary>
    /// Assetパスからフォルダパスを取得します。
    /// </summary>
    /// <param name="f_assetPath">Assetパス</param>
    /// <param name="f_defaultFolderPath">取得できなかった場合の初期フォルダ</param>
    /// <returns>フォルダパス</returns>
    private string GetFolderPathFromAssetPath(
        string f_assetPath,
        string f_defaultFolderPath)
    {
        if (string.IsNullOrEmpty(f_assetPath))
        {
            return f_defaultFolderPath;
        }

        string folderPath = System.IO.Path.GetDirectoryName(f_assetPath);

        if (string.IsNullOrEmpty(folderPath))
        {
            return f_defaultFolderPath;
        }

        return folderPath.Replace("\\", "/");
    }

    /// <summary>
    /// 空文字の場合に初期値を返します。
    /// </summary>
    /// <param name="f_text">確認する文字列</param>
    /// <param name="f_defaultText">初期値</param>
    /// <returns>使用する文字列</returns>
    private string GetLoadText(string f_text, string f_defaultText)
    {
        if (string.IsNullOrEmpty(f_text))
        {
            return f_defaultText;
        }

        return f_text;
    }

    /// <summary>
    /// 現在のFieldData一覧を保存用データに変換します。
    /// </summary>
    /// <returns>保存用FieldData一覧</returns>
    private List<CSED_CreateTools_FieldDataSaveData> CreateCurrentFieldDataSaveDataList()
    {
        List<CSED_CreateTools_FieldDataSaveData> saveDataList =
            new List<CSED_CreateTools_FieldDataSaveData>();

        if (m_FieldDataList == null)
        {
            return saveDataList;
        }

        for (int i = 0 ; i < m_FieldDataList.Count ; i++)
        {
            saveDataList.Add(CreateFieldDataSaveData(m_FieldDataList[i]));
        }

        return saveDataList;
    }

    /// <summary>
    /// FieldDataを保存用データに変換します。
    /// </summary>
    /// <param name="f_fieldData">変換元FieldData</param>
    /// <returns>保存用データ</returns>
    private CSED_CreateTools_FieldDataSaveData CreateFieldDataSaveData(CSED_CreateTools_FieldData f_fieldData)
    {
        CSED_CreateTools_FieldDataSaveData saveData =
            new CSED_CreateTools_FieldDataSaveData();

        saveData.fieldType = f_fieldData.FieldType;
        saveData.fieldName = f_fieldData.FieldName;
        saveData.fieldLayoutType = f_fieldData.FieldLayoutType;
        saveData.tagName = f_fieldData.TagName;

        saveData.listElementFieldType = f_fieldData.ListElementFieldType;
        saveData.isListDefaultValueNull = f_fieldData.IsListDefaultValueNull;
        saveData.isListDefaultCountNull = f_fieldData.IsListDefaultCountNull;
        saveData.listDefaultCountText = f_fieldData.ListDefaultCountText;

        saveData.isDefaultValueNull = f_fieldData.IsDefaultValueNull;
        saveData.defaultValueText = f_fieldData.DefaultValueText;

        saveData.isDefaultMinValueNull = f_fieldData.IsDefaultMinValueNull;
        saveData.defaultMinValueText = f_fieldData.DefaultMinValueText;

        saveData.isDefaultMaxValueNull = f_fieldData.IsDefaultMaxValueNull;
        saveData.defaultMaxValueText = f_fieldData.DefaultMaxValueText;

        saveData.isSliderMinValueNull = f_fieldData.IsSliderMinValueNull;
        saveData.sliderMinValueText = f_fieldData.SliderMinValueText;

        saveData.isSliderMaxValueNull = f_fieldData.IsSliderMaxValueNull;
        saveData.sliderMaxValueText = f_fieldData.SliderMaxValueText;

        saveData.scriptableObjectTypeScript = f_fieldData.ScriptableObjectTypeScript;
        saveData.defaultScriptableObjectValue = f_fieldData.DefaultScriptableObjectValue;
        saveData.defaultScriptValue = f_fieldData.DefaultScriptValue;
        saveData.defaultGameObjectValue = f_fieldData.DefaultGameObjectValue;

        saveData.listDefaultElementValueTextList =
            new List<string>(f_fieldData.ListDefaultElementValueTextList);

        saveData.listDefaultMinValueTextList =
            new List<string>(f_fieldData.ListDefaultMinValueTextList);

        saveData.listDefaultMaxValueTextList =
            new List<string>(f_fieldData.ListDefaultMaxValueTextList);

        saveData.listDefaultObjectValueList =
            new List<UnityEngine.Object>(f_fieldData.ListDefaultObjectValueList);

        return saveData;
    }

    /// <summary>
    /// 保存用FieldData一覧からFieldData一覧を復元します。
    /// </summary>
    /// <param name="f_saveDataList">保存用FieldData一覧</param>
    /// <returns>復元後FieldData一覧</returns>
    private List<CSED_CreateTools_FieldData> CreateFieldDataListFromSaveDataList(
        List<CSED_CreateTools_FieldDataSaveData> f_saveDataList)
    {
        List<CSED_CreateTools_FieldData> fieldDataList =
            new List<CSED_CreateTools_FieldData>();

        if (f_saveDataList == null)
        {
            return fieldDataList;
        }

        for (int i = 0 ; i < f_saveDataList.Count ; i++)
        {
            fieldDataList.Add(CreateFieldDataFromSaveData(f_saveDataList[i]));
        }

        return fieldDataList;
    }

    /// <summary>
    /// 保存用FieldDataからFieldDataを復元します。
    /// </summary>
    /// <param name="f_saveData">保存用FieldData</param>
    /// <returns>復元後FieldData</returns>
    private CSED_CreateTools_FieldData CreateFieldDataFromSaveData(
        CSED_CreateTools_FieldDataSaveData f_saveData)
    {
        CSED_CreateTools_FieldData fieldData =
            new CSED_CreateTools_FieldData(
                f_saveData.fieldType,
                f_saveData.fieldName,
                f_saveData.fieldLayoutType);

        fieldData.TagName = f_saveData.tagName;

        fieldData.ListElementFieldType = f_saveData.listElementFieldType;
        fieldData.IsListDefaultValueNull = f_saveData.isListDefaultValueNull;
        fieldData.IsListDefaultCountNull = f_saveData.isListDefaultCountNull;
        fieldData.ListDefaultCountText = f_saveData.listDefaultCountText;

        fieldData.IsDefaultValueNull = f_saveData.isDefaultValueNull;
        fieldData.DefaultValueText = f_saveData.defaultValueText;

        fieldData.IsDefaultMinValueNull = f_saveData.isDefaultMinValueNull;
        fieldData.DefaultMinValueText = f_saveData.defaultMinValueText;

        fieldData.IsDefaultMaxValueNull = f_saveData.isDefaultMaxValueNull;
        fieldData.DefaultMaxValueText = f_saveData.defaultMaxValueText;

        fieldData.IsSliderMinValueNull = f_saveData.isSliderMinValueNull;
        fieldData.SliderMinValueText = f_saveData.sliderMinValueText;

        fieldData.IsSliderMaxValueNull = f_saveData.isSliderMaxValueNull;
        fieldData.SliderMaxValueText = f_saveData.sliderMaxValueText;

        fieldData.ScriptableObjectTypeScript = f_saveData.scriptableObjectTypeScript;
        fieldData.DefaultScriptableObjectValue = f_saveData.defaultScriptableObjectValue;
        fieldData.DefaultScriptValue = f_saveData.defaultScriptValue;
        fieldData.DefaultGameObjectValue = f_saveData.defaultGameObjectValue;

        fieldData.ListDefaultElementValueTextList =
            new List<string>(f_saveData.listDefaultElementValueTextList);

        fieldData.ListDefaultMinValueTextList =
            new List<string>(f_saveData.listDefaultMinValueTextList);

        fieldData.ListDefaultMaxValueTextList =
            new List<string>(f_saveData.listDefaultMaxValueTextList);

        fieldData.ListDefaultObjectValueList =
            new List<UnityEngine.Object>(f_saveData.listDefaultObjectValueList);

        return fieldData;
    }
}
#endif
