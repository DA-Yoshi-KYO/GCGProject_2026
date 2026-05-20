/*
+=====================================
 ファイル名 : CSED_CreateTools_GeneratedToolCreator.cs
 概要     : CreateToolsのEditorWindow生成実行処理をまとめるクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/05/18 新規作成
            2026/05/18 EditorWindowとScriptableObject箱を同時生成する処理を追加
=====================================+
*/

#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;

/// <summary>
/// CreateToolsのEditorWindow生成実行処理をまとめるクラスです。
/// </summary>
public partial class CSED_CreateTools
{
    /// <summary>
    /// EditorWindowスクリプトとScriptableObjectスクリプトを生成します。
    /// </summary>
    private void CreateGeneratedEditorTool()
    {
        if (ValidateGeneratedToolCreateSettings() == false)
        {
            return;
        }

        string safeEditorClassName = SanitizeClassName(m_GeneratedToolClassName);
        string safeDataClassName = SanitizeClassName(m_GeneratedScriptableObjectClassName);

        if (Directory.Exists(m_GeneratedToolOutputFolderPath) == false)
        {
            Directory.CreateDirectory(m_GeneratedToolOutputFolderPath);
        }

        if (Directory.Exists(m_GeneratedScriptableObjectOutputFolderPath) == false)
        {
            Directory.CreateDirectory(m_GeneratedScriptableObjectOutputFolderPath);
        }

        string editorOutputFilePath = Path.Combine(
            m_GeneratedToolOutputFolderPath,
            safeEditorClassName + ".cs");

        string dataOutputFilePath = Path.Combine(
            m_GeneratedScriptableObjectOutputFolderPath,
            safeDataClassName + ".cs");

        File.WriteAllText(
            editorOutputFilePath,
            CreateGeneratedEditorToolSource(safeEditorClassName, safeDataClassName),
            Encoding.UTF8);

        File.WriteAllText(
            dataOutputFilePath,
            CreateGeneratedScriptableObjectSource(safeDataClassName),
            Encoding.UTF8);

        SaveGeneratedEditorRecord(
            safeEditorClassName,
            safeDataClassName,
            editorOutputFilePath,
            dataOutputFilePath);

        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Create Complete",
            "EditorToolとScriptableObjectの箱を生成しました。\n\n"
            + editorOutputFilePath
            + "\n"
            + dataOutputFilePath,
            "OK");
    }

    /// <summary>
    /// 生成したEditor情報を一覧アセットへ保存します。
    /// </summary>
    /// <param name="f_editorClassName">EditorWindowクラス名</param>
    /// <param name="f_dataClassName">ScriptableObjectクラス名</param>
    /// <param name="f_editorScriptPath">EditorWindowスクリプトパス</param>
    /// <param name="f_dataScriptPath">ScriptableObjectスクリプトパス</param>
    private void SaveGeneratedEditorRecord(
        string f_editorClassName,
        string f_dataClassName,
        string f_editorScriptPath,
        string f_dataScriptPath)
    {
        CSS_CreateToolsGeneratedEditorList editorList = LoadOrCreateGeneratedEditorList();

        string nowDate = System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

        CSED_CreateTools_GeneratedEditorRecord record =
            new CSED_CreateTools_GeneratedEditorRecord();

        record.titleName = m_GeneratedToolWindowTitle;
        record.authorName = m_GeneratedHeaderAuthorName;

        record.editorClassName = f_editorClassName;
        record.dataClassName = f_dataClassName;
        record.menuPath = m_GeneratedToolMenuPath;

        record.editorScriptPath = f_editorScriptPath;
        record.dataScriptPath = f_dataScriptPath;

        record.headerHistoryDate = m_GeneratedHeaderHistoryDate;
        record.editorHeaderContents = m_GeneratedEditorHeaderContents;
        record.dataHeaderContents = m_GeneratedDataHeaderContents;

        record.defaultAssetName = m_GeneratedDefaultAssetName;
        record.defaultAssetFolder = m_GeneratedDefaultAssetFolder;

        record.fieldSaveDataList = CreateCurrentFieldDataSaveDataList();

        int sameIndex = editorList.generatedEditorRecordList.FindIndex(
            item => item.editorClassName == f_editorClassName);

        if (sameIndex >= 0)
        {
            CSED_CreateTools_GeneratedEditorRecord oldRecord =
                editorList.generatedEditorRecordList[sameIndex];

            record.createdDate = oldRecord.createdDate;

            if (string.IsNullOrEmpty(record.createdDate))
            {
                record.createdDate = nowDate;
            }

            record.updatedDate = nowDate;

            editorList.generatedEditorRecordList[sameIndex] = record;
        }
        else
        {
            record.createdDate = nowDate;
            record.updatedDate = nowDate;

            editorList.generatedEditorRecordList.Add(record);
        }

        EditorUtility.SetDirty(editorList);
        AssetDatabase.SaveAssets();
    }

    /// <summary>
    /// 生成済みEditor一覧アセットを読み込みます。
    /// 存在しない場合は作成します。
    /// </summary>
    /// <returns>生成済みEditor一覧アセット</returns>
    private CSS_CreateToolsGeneratedEditorList LoadOrCreateGeneratedEditorList()
    {
        const string assetPath = "Assets/Editor/CreateTools/CSS_CreateToolsGeneratedEditorList.asset";

        CSS_CreateToolsGeneratedEditorList editorList =
            AssetDatabase.LoadAssetAtPath<CSS_CreateToolsGeneratedEditorList>(assetPath);

        if (editorList != null)
        {
            return editorList;
        }

        string folderPath = System.IO.Path.GetDirectoryName(assetPath);

        if (System.IO.Directory.Exists(folderPath) == false)
        {
            System.IO.Directory.CreateDirectory(folderPath);
        }

        editorList = CreateInstance<CSS_CreateToolsGeneratedEditorList>();

        AssetDatabase.CreateAsset(editorList, assetPath);
        AssetDatabase.SaveAssets();

        return editorList;
    }

    /// <summary>
    /// EditorTool生成前の設定を確認します。
    /// </summary>
    /// <returns>生成可能な場合はtrue</returns>
    private bool ValidateGeneratedToolCreateSettings()
    {
        StringBuilder errorMessageBuilder = new StringBuilder();

        AppendIdentifierErrorMessage(
            errorMessageBuilder,
            "Editor Class",
            m_GeneratedToolClassName);

        AppendIdentifierErrorMessage(
            errorMessageBuilder,
            "Data Class",
            m_GeneratedScriptableObjectClassName);

        if (m_FieldDataList != null)
        {
            for (int i = 0 ; i < m_FieldDataList.Count ; i++)
            {
                CSED_CreateTools_FieldData fieldData = m_FieldDataList[i];

                AppendIdentifierErrorMessage(
                    errorMessageBuilder,
                    "Field " + (i + 1).ToString() + " Variable Name",
                    fieldData.FieldName);
            }
        }

        if (errorMessageBuilder.Length <= 0)
        {
            return true;
        }

        EditorUtility.DisplayDialog(
            "Create Editor Tool Error",
            "生成できない名前が含まれています。\n\n"
            + "Editor Class / Data Class / Variable Name には、\n"
            + "半角英字・半角数字・アンダースコアのみ使用できます。\n"
            + "また、先頭に数字は使用できません。\n\n"
            + errorMessageBuilder.ToString(),
            "OK");

        return false;
    }

    /// <summary>
    /// 識別子として不正な場合、エラーメッセージを追加します。
    /// </summary>
    /// <param name="f_builder">エラーメッセージ</param>
    /// <param name="f_itemName">項目名</param>
    /// <param name="f_value">確認する文字列</param>
    private void AppendIdentifierErrorMessage(
        StringBuilder f_builder,
        string f_itemName,
        string f_value)
    {
        if (IsValidGeneratedIdentifier(f_value))
        {
            return;
        }

        f_builder.AppendLine(
            "・" + f_itemName + " : " + GetDisplayInvalidIdentifierText(f_value));
    }

    /// <summary>
    /// 生成コードで使用できる識別子か確認します。
    /// </summary>
    /// <param name="f_text">確認する文字列</param>
    /// <returns>使用可能な場合はtrue</returns>
    private bool IsValidGeneratedIdentifier(string f_text)
    {
        if (string.IsNullOrEmpty(f_text))
        {
            return false;
        }

        if (IsAsciiLetter(f_text[0]) == false && f_text[0] != '_')
        {
            return false;
        }

        for (int i = 1 ; i < f_text.Length ; i++)
        {
            char currentChar = f_text[i];

            if (IsAsciiLetter(currentChar) ||
                IsAsciiDigit(currentChar) ||
                currentChar == '_')
            {
                continue;
            }

            return false;
        }

        return true;
    }

    /// <summary>
    /// 半角英字か確認します。
    /// </summary>
    /// <param name="f_char">確認する文字</param>
    /// <returns>半角英字の場合はtrue</returns>
    private bool IsAsciiLetter(char f_char)
    {
        return
            (f_char >= 'a' && f_char <= 'z') ||
            (f_char >= 'A' && f_char <= 'Z');
    }

    /// <summary>
    /// 半角数字か確認します。
    /// </summary>
    /// <param name="f_char">確認する文字</param>
    /// <returns>半角数字の場合はtrue</returns>
    private bool IsAsciiDigit(char f_char)
    {
        return f_char >= '0' && f_char <= '9';
    }

    /// <summary>
    /// エラー表示用の識別子文字列を取得します。
    /// </summary>
    /// <param name="f_text">表示する文字列</param>
    /// <returns>表示用文字列</returns>
    private string GetDisplayInvalidIdentifierText(string f_text)
    {
        if (string.IsNullOrEmpty(f_text))
        {
            return "(空欄)";
        }

        return f_text;
    }
}
#endif
