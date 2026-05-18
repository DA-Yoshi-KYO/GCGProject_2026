/*
+=====================================
 ファイル名 : CSED_CreateTools_SystemState.cs
 概要     : CreateToolsのシステム状態管理クラス
 作者     : ヨシモト リョウ
 履歴     : 2026/04/22 新規作成
            2026/05/08 中央エリア用の変数データリストを追加
=====================================+
*/

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CreateToolsのシステム状態をまとめるクラスです。
/// </summary>
public partial class CSED_CreateTools
{
    /// <summary>
    /// 生成するScriptableObjectクラス名です。
    /// </summary>
    private string m_GeneratedScriptableObjectClassName = "CSS_NewToolData";

    /// <summary>
    /// 生成するScriptableObjectスクリプトの出力先フォルダです。
    /// </summary>
    private string m_GeneratedScriptableObjectOutputFolderPath = "Assets/Programmer/Scripts/GeneratedData";

    /// <summary>
    /// 生成されたEditorToolから作成するScriptableObjectアセットの保存先です。
    /// </summary>
    private string m_GeneratedAssetOutputFolderPath = "Assets/ScriptableObject/GeneratedData";

    /// <summary>
    /// 生成するEditorWindowのタイトルです。
    /// </summary>
    private string m_GeneratedToolWindowTitle = "New Tool";

    /// <summary>
    /// 生成するEditorWindowのクラス名です。
    /// </summary>
    private string m_GeneratedToolClassName = "CSED_NewToolWindow";

    /// <summary>
    /// 生成したEditorWindowを表示するUnityメニューパスです。
    /// </summary>
    private string m_GeneratedToolMenuPath = "Tools/Generated/New Tool";

    /// <summary>
    /// 生成するEditorWindowスクリプトの出力先フォルダです。
    /// </summary>
    private string m_GeneratedToolOutputFolderPath = "Assets/Editor/GeneratedCreateTools";

    /// <summary>
    /// 左ウィンドウ現在横幅です。
    /// </summary>
    private float m_LeftCurrentWidth;

    /// <summary>
    /// 右ウィンドウ現在横幅です。
    /// </summary>
    private float m_RightCurrentWidth;

    /// <summary>
    /// 左上エリア現在高さです。
    /// </summary>
    private float m_LeftTopCurrentHeight;

    /// <summary>
    /// 左右分割初期化済みフラグです。
    /// </summary>
    private bool m_IsHorizontalInitialized;

    /// <summary>
    /// 左内部上下分割初期化済みフラグです。
    /// </summary>
    private bool m_IsVerticalInitialized;

    /// <summary>
    /// 左右分割の左バーをドラッグ中かどうかです。
    /// </summary>
    private bool m_IsDraggingLeftHorizontal;

    /// <summary>
    /// 左右分割の右バーをドラッグ中かどうかです。
    /// </summary>
    private bool m_IsDraggingRightHorizontal;

    /// <summary>
    /// 左内部上下バーをドラッグ中かどうかです。
    /// </summary>
    private bool m_IsDraggingLeftVertical;

    /// <summary>
    /// 横方向ドラッグ補正値です。
    /// </summary>
    private float m_HorizontalDragOffset;

    /// <summary>
    /// 縦方向ドラッグ補正値です。
    /// </summary>
    private float m_VerticalDragOffset;

    /// <summary>
    /// 中央エリアに配置された変数データリストです。
    /// </summary>
    private List<CSED_CreateTools_FieldData> m_FieldDataList = new List<CSED_CreateTools_FieldData>();

    /// <summary>
    /// 中央エリアのスクロール位置です。
    /// </summary>
    private Vector2 m_FieldCanvasScrollPosition;

    /// <summary>
    /// 現在選択中のField番号です。
    /// </summary>
    private int m_SelectedFieldDataIndex = -1;

    /// <summary>
    /// 左下Field詳細設定エリアのスクロール位置です。
    /// </summary>
    private Vector2 m_FieldInspectorScrollPosition;

    /// <summary>
    /// 右側プレビューエリアのスクロール位置です。
    /// </summary>
    private Vector2 m_PreviewScrollPosition;

    /// <summary>
    /// 右側プレビューの仮想EditorWindowタイトル名です。
    /// </summary>
    private string m_PreviewEditorTitleName = "Test";

    /// <summary>
    /// 右側プレビューのエディター設定を表示中かどうかです。
    /// </summary>
    private bool m_IsPreviewEditorSettingsOpen;
}
#endif
