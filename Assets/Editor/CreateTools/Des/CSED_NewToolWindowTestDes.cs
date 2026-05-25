/*
+=====================================
 ファイル名 : CSED_NewToolWindowTestDes.cs
 概要     : CreateToolsから自動生成されたEditorWindowデス
 作者     : ヨシモト リョウデス
 履歴     : 2026/05/22デス CreateToolsから自動生成
=====================================+
*/

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// CreateToolsから自動生成されたEditorWindowです。
/// </summary>
public class CSED_NewToolWindowTestDes : EditorWindow, IHasCustomMenu
{
    /// <summary>
    /// newVector2Field01です。
    /// </summary>
    private Vector2 newVector2Field01 = Vector2.zero;

    /// <summary>
    /// newVector2IntField02です。
    /// </summary>
    private Vector2Int newVector2IntField02 = Vector2Int.zero;

    /// <summary>
    /// newField03です。
    /// </summary>
    private GameObject newField03 = null;

    /// <summary>
    /// newListField04です。
    /// </summary>
    private List<Vector2> newListField04 = new List<Vector2>()
    {};

    /// <summary>
    /// newListField05です。
    /// </summary>
    private List<Vector2Int> newListField05 = new List<Vector2Int>()
    {};

    /// <summary>
    /// メイン画面のスクロール位置です。
    /// </summary>
    private Vector2 m_MainScrollPosition;

    /// <summary>
    /// メニューからウィンドウを開きます。
    /// </summary>
    [MenuItem("Tools/New Toolデス")]
    public static void ShowWindow()
    {
        CSED_NewToolWindowTestDes window = GetWindow<CSED_NewToolWindowTestDes>("TestEditorデス");
        window.minSize = new Vector2(360.0f, 240.0f);
        CSED_NewToolWindowTestDes_CreatedAssetsWindow.OpenWindow();
        window.Focus();
    }

    /// <summary>
    /// EditorWindow右上メニューに項目を追加します。
    /// </summary>
    /// <param name="f_menu">追加先メニュー</param>
    public void AddItemsToMenu(GenericMenu f_menu)
    {
        f_menu.AddItem(
            new GUIContent("Create Asset Settings"),
            false,
            OpenCreateAssetSettings);
    }

    /// <summary>
    /// 作成済みAsset一覧Windowを開きます。
    /// </summary>
    private void OpenCreatedAssetsWindow()
    {
        CSED_NewToolWindowTestDes_CreatedAssetsWindow.OpenWindow();
    }

    /// <summary>
    /// Create Asset設定を開きます。
    /// </summary>
    private void OpenCreateAssetSettings()
    {
        CreateAssetSettingsWindow.Open(this);
    }

    /// <summary>
    /// GUIを描画します。
    /// </summary>
    private void OnGUI()
    {
        m_MainScrollPosition = EditorGUILayout.BeginScrollView(m_MainScrollPosition);
        {
        newVector2Field01 = EditorGUILayout.Vector2Field("newVector2Field01", newVector2Field01);
        GUILayout.Space(6.0f);

        newVector2IntField02 = EditorGUILayout.Vector2IntField("newVector2IntField02", newVector2IntField02);
        GUILayout.Space(6.0f);

        newField03 = (GameObject)EditorGUILayout.ObjectField("newField03", newField03, typeof(GameObject), false);
        GUILayout.Space(6.0f);

        EditorGUILayout.LabelField("newListField04", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("-", GUILayout.Width(24.0f)) && newListField04.Count > 0)
            {
                newListField04.RemoveAt(newListField04.Count - 1);
            }

            EditorGUILayout.LabelField(newListField04.Count.ToString(), GUILayout.Width(32.0f));

            if (GUILayout.Button("+", GUILayout.Width(24.0f)))
            {
                newListField04.Add(Vector2.zero);
            }
        }
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < newListField04.Count; i++)
        {
            newListField04[i] = EditorGUILayout.Vector2Field("Element " + i.ToString(), newListField04[i]);
        }
        GUILayout.Space(6.0f);

        EditorGUILayout.LabelField("newListField05", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("-", GUILayout.Width(24.0f)) && newListField05.Count > 0)
            {
                newListField05.RemoveAt(newListField05.Count - 1);
            }

            EditorGUILayout.LabelField(newListField05.Count.ToString(), GUILayout.Width(32.0f));

            if (GUILayout.Button("+", GUILayout.Width(24.0f)))
            {
                newListField05.Add(Vector2Int.zero);
            }
        }
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < newListField05.Count; i++)
        {
            newListField05[i] = EditorGUILayout.Vector2IntField("Element " + i.ToString(), newListField05[i]);
        }
        GUILayout.Space(6.0f);

        GUILayout.Space(12.0f);
        EditorGUILayout.LabelField("Create Asset", EditorStyles.boldLabel);

        if (GUILayout.Button("Create ScriptableObject", GUILayout.Height(28.0f)))
        {
            CreateScriptableObjectAsset();
        }
        }
        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// 作成するScriptableObjectアセット名です。
    /// </summary>
    private string m_AssetFileName = "Des";

    /// <summary>
    /// ScriptableObjectアセットの保存先です。
    /// </summary>
    private string m_AssetOutputFolderPath = "Assets/Programmer/ScriptableObject/Des";

    /// <summary>
    /// ScriptableObjectアセットを作成します。
    /// </summary>
    private void CreateScriptableObjectAsset()
    {
        if (System.IO.Directory.Exists(m_AssetOutputFolderPath) == false)
        {
            System.IO.Directory.CreateDirectory(m_AssetOutputFolderPath);
        }

        CSS_NewToolDatTestDes asset = CreateInstance<CSS_NewToolDatTestDes>();
        asset.newVector2Field01 = newVector2Field01;
        asset.newVector2IntField02 = newVector2IntField02;
        asset.newField03 = newField03;
        asset.newListField04 = new List<Vector2>(newListField04);
        asset.newListField05 = new List<Vector2Int>(newListField05);

        string assetPath = AssetDatabase.GenerateUniqueAssetPath(
            System.IO.Path.Combine(m_AssetOutputFolderPath, m_AssetFileName + ".asset"));

        AssetDatabase.CreateAsset(asset, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = asset;
        CSED_NewToolWindowTestDes_CreatedAssetsWindow.RepaintOpenedWindows();
    }

    /// <summary>
    /// Create Asset設定専用のEditorWindowです。
    /// </summary>
    private class CreateAssetSettingsWindow : EditorWindow
    {
        /// <summary>
        /// 設定対象のEditorWindowです。
        /// </summary>
        private CSED_NewToolWindowTestDes m_OwnerWindow;

        /// <summary>
        /// Create Asset設定Windowを開きます。
        /// </summary>
        /// <param name="f_ownerWindow">設定対象のEditorWindow</param>
        public static void Open(CSED_NewToolWindowTestDes f_ownerWindow)
        {
            CreateAssetSettingsWindow window = CreateInstance<CreateAssetSettingsWindow>();
            window.titleContent = new GUIContent("Create Asset Settings");
            window.m_OwnerWindow = f_ownerWindow;
            window.minSize = new Vector2(360.0f, 120.0f);
            window.position = new Rect(
                f_ownerWindow.position.x + 40.0f,
                f_ownerWindow.position.y + 40.0f,
                360.0f,
                120.0f);
            window.ShowUtility();
        }

        /// <summary>
        /// GUIを描画します。
        /// </summary>
        private void OnGUI()
        {
            if (m_OwnerWindow == null)
            {
                EditorGUILayout.HelpBox("設定対象のEditorWindowが見つかりません。", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField("Create Asset Settings", EditorStyles.boldLabel);
            GUILayout.Space(6.0f);

            m_OwnerWindow.m_AssetFileName = EditorGUILayout.TextField("Asset Name", m_OwnerWindow.m_AssetFileName);
            m_OwnerWindow.m_AssetOutputFolderPath = EditorGUILayout.TextField("Asset Folder", m_OwnerWindow.m_AssetOutputFolderPath);

            if (GUI.changed)
            {
                m_OwnerWindow.Repaint();
            }
        }
    }

}

/// <summary>
/// CSS_NewToolDatTestDesで作成されたScriptableObject一覧を表示するEditorWindowです。
/// </summary>
public class CSED_NewToolWindowTestDes_CreatedAssetsWindow : EditorWindow
{
    /// <summary>
    /// Asset一覧のスクロール位置です。
    /// </summary>
    private Vector2 m_AssetListScrollPosition;

    /// <summary>
    /// 表示用にキャッシュしたAssetパス一覧です。
    /// </summary>
    private List<string> m_CachedAssetPathList = new List<string>();

    /// <summary>
    /// Assetごとの設定表示状態です。
    /// </summary>
    private Dictionary<string, bool> m_AssetFoldoutStateDictionary = new Dictionary<string, bool>();

    /// <summary>
    /// Created Assetsウィンドウを開きます。
    /// </summary>
    public static void OpenWindow()
    {
        CSED_NewToolWindowTestDes_CreatedAssetsWindow window = GetWindow<CSED_NewToolWindowTestDes_CreatedAssetsWindow>(
            "Created Assets",
            false,
            typeof(CSED_NewToolWindowTestDes));

        window.minSize = new Vector2(420.0f, 300.0f);
    }

    /// <summary>
    /// 開いているCreated Assetsウィンドウを再描画します。
    /// </summary>
    public static void RepaintOpenedWindows()
    {
        CSED_NewToolWindowTestDes_CreatedAssetsWindow[] windows = Resources.FindObjectsOfTypeAll<CSED_NewToolWindowTestDes_CreatedAssetsWindow>();

        for (int i = 0; i < windows.Length; i++)
        {
            windows[i].RefreshAssetPathList();
            windows[i].Repaint();
        }
    }

    /// <summary>
    /// Window有効化時にAsset一覧を更新します。
    /// </summary>
    private void OnEnable()
    {
        RefreshAssetPathList();
    }

    /// <summary>
    /// Asset一覧を更新します。
    /// </summary>
    private void RefreshAssetPathList()
    {
        m_CachedAssetPathList.Clear();

        string[] assetGuids = AssetDatabase.FindAssets("t:" + nameof(CSS_NewToolDatTestDes));

        for (int i = 0; i < assetGuids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGuids[i]);
            m_CachedAssetPathList.Add(assetPath);
        }

        m_CachedAssetPathList.Sort(CompareAssetPathByNaturalName);
    }

    /// <summary>
    /// GUIを描画します。
    /// </summary>
    private void OnGUI()
    {
        DrawCreatedAssetList();
    }

    /// <summary>
    /// 作成済みAsset一覧を描画します。
    /// </summary>
    private void DrawCreatedAssetList()
    {
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("Created ScriptableObjects", EditorStyles.boldLabel);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Refresh", GUILayout.Width(80.0f), GUILayout.Height(20.0f)))
            {
                RefreshAssetPathList();
            }
        }
        EditorGUILayout.EndHorizontal();

        if (m_CachedAssetPathList.Count <= 0)
        {
            EditorGUILayout.HelpBox("まだ作成済みAssetがありません。", MessageType.Info);
            return;
        }

        m_AssetListScrollPosition = EditorGUILayout.BeginScrollView(m_AssetListScrollPosition);
        {
            for (int i = 0; i < m_CachedAssetPathList.Count; i++)
            {
                string assetPath = m_CachedAssetPathList[i];
                CSS_NewToolDatTestDes asset = AssetDatabase.LoadAssetAtPath<CSS_NewToolDatTestDes>(assetPath);

                if (asset == null)
                {
                    continue;
                }

                DrawCreatedAssetListItem(asset, assetPath);
                GUILayout.Space(10.0f);
            }
        }
        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// AssetパスをAsset名の自然順で比較します。
    /// </summary>
    /// <param name="f_leftPath">左側Assetパス</param>
    /// <param name="f_rightPath">右側Assetパス</param>
    /// <returns>比較結果</returns>
    private int CompareAssetPathByNaturalName(string f_leftPath, string f_rightPath)
    {
        string leftName = System.IO.Path.GetFileNameWithoutExtension(f_leftPath);
        string rightName = System.IO.Path.GetFileNameWithoutExtension(f_rightPath);

        int nameCompare = CompareNaturalText(leftName, rightName);

        if (nameCompare != 0)
        {
            return nameCompare;
        }

        return string.Compare(f_leftPath, f_rightPath, System.StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 文字列を自然順で比較します。
    /// a1, a2, a10 のように数字部分を数値として比較します。
    /// </summary>
    /// <param name="f_leftText">左側文字列</param>
    /// <param name="f_rightText">右側文字列</param>
    /// <returns>比較結果</returns>
    private int CompareNaturalText(string f_leftText, string f_rightText)
    {
        int leftIndex = 0;
        int rightIndex = 0;

        while (leftIndex < f_leftText.Length && rightIndex < f_rightText.Length)
        {
            char leftChar = f_leftText[leftIndex];
            char rightChar = f_rightText[rightIndex];

            if (char.IsDigit(leftChar) && char.IsDigit(rightChar))
            {
                int numberCompare = CompareNaturalNumberPart(f_leftText, ref leftIndex, f_rightText, ref rightIndex);

                if (numberCompare != 0)
                {
                    return numberCompare;
                }

                continue;
            }

            int charCompare = string.Compare(
                leftChar.ToString(),
                rightChar.ToString(),
                true,
                System.Globalization.CultureInfo.GetCultureInfo("ja-JP"));

            if (charCompare != 0)
            {
                return charCompare;
            }

            leftIndex++;
            rightIndex++;
        }

        return f_leftText.Length.CompareTo(f_rightText.Length);
    }

    /// <summary>
    /// 文字列内の数字部分を数値として比較します。
    /// </summary>
    /// <param name="f_leftText">左側文字列</param>
    /// <param name="f_leftIndex">左側現在位置</param>
    /// <param name="f_rightText">右側文字列</param>
    /// <param name="f_rightIndex">右側現在位置</param>
    /// <returns>比較結果</returns>
    private int CompareNaturalNumberPart(
        string f_leftText,
        ref int f_leftIndex,
        string f_rightText,
        ref int f_rightIndex)
    {
        long leftNumber = ReadNaturalNumber(f_leftText, ref f_leftIndex);
        long rightNumber = ReadNaturalNumber(f_rightText, ref f_rightIndex);

        return leftNumber.CompareTo(rightNumber);
    }

    /// <summary>
    /// 文字列内の数字部分を読み取ります。
    /// </summary>
    /// <param name="f_text">対象文字列</param>
    /// <param name="f_index">現在位置</param>
    /// <returns>読み取った数値</returns>
    private long ReadNaturalNumber(string f_text, ref int f_index)
    {
        long number = 0;

        while (f_index < f_text.Length && char.IsDigit(f_text[f_index]))
        {
            number = number * 10 + (f_text[f_index] - '0');
            f_index++;
        }

        return number;
    }

    /// <summary>
    /// Asset一覧の1項目を描画します。
    /// </summary>
    /// <param name="f_asset">対象Asset</param>
    /// <param name="f_assetPath">対象Assetパス</param>
    private void DrawCreatedAssetListItem(CSS_NewToolDatTestDes f_asset, string f_assetPath)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.LabelField(f_asset.name, EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Path", GUILayout.Width(36.0f));
                EditorGUILayout.TextField(f_assetPath);

                string foldoutButtonText = IsAssetFoldoutOpened(f_assetPath) ? "▼" : "▶";

                if (GUILayout.Button(foldoutButtonText, GUILayout.Width(24.0f), GUILayout.Height(20.0f)))
                {
                    ToggleAssetFoldout(f_assetPath);
                }

                if (GUILayout.Button("Select", GUILayout.Width(80.0f), GUILayout.Height(20.0f)))
                {
                    SelectAsset(f_asset);
                }
            }
            EditorGUILayout.EndHorizontal();

            if (IsAssetFoldoutOpened(f_assetPath))
            {
                GUILayout.Space(4.0f);
                DrawCreatedAssetSettings(f_asset, f_assetPath);
            }
        }
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// Asset設定表示状態を切り替えます。
    /// </summary>
    /// <param name="f_assetPath">対象Assetパス</param>
    private void ToggleAssetFoldout(string f_assetPath)
    {
        if (m_AssetFoldoutStateDictionary.ContainsKey(f_assetPath) == false)
        {
            m_AssetFoldoutStateDictionary.Add(f_assetPath, true);
            return;
        }

        m_AssetFoldoutStateDictionary[f_assetPath] = !m_AssetFoldoutStateDictionary[f_assetPath];
    }

    /// <summary>
    /// Asset設定表示状態を取得します。
    /// </summary>
    /// <param name="f_assetPath">対象Assetパス</param>
    /// <returns>表示中ならtrue</returns>
    private bool IsAssetFoldoutOpened(string f_assetPath)
    {
        if (m_AssetFoldoutStateDictionary.ContainsKey(f_assetPath) == false)
        {
            return false;
        }

        return m_AssetFoldoutStateDictionary[f_assetPath];
    }

    /// <summary>
    /// 作成済みAssetの設定項目を描画します。
    /// </summary>
    /// <param name="f_asset">対象Asset</param>
    /// <param name="f_assetPath">対象Assetパス</param>
    private void DrawCreatedAssetSettings(CSS_NewToolDatTestDes f_asset, string f_assetPath)
    {
        GUILayout.Space(4.0f);

        EditorGUI.BeginChangeCheck();
        string newAssetName = EditorGUILayout.DelayedTextField("Asset Name", f_asset.name);

        if (EditorGUI.EndChangeCheck())
        {
            if (string.IsNullOrEmpty(newAssetName) == false && newAssetName != f_asset.name)
            {
                string renameError = AssetDatabase.RenameAsset(f_assetPath, newAssetName);

                if (string.IsNullOrEmpty(renameError))
                {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    RefreshAssetPathList();
                    Repaint();
                    GUIUtility.ExitGUI();
                }
                else
                {
                    EditorUtility.DisplayDialog("Rename Asset Error", renameError, "OK");
                }
            }
        }

        SerializedObject serializedObject = new SerializedObject(f_asset);
        serializedObject.Update();

        DrawCreatedAssetSerializedFields(serializedObject);

        if (serializedObject.ApplyModifiedProperties())
        {
            EditorUtility.SetDirty(f_asset);
            AssetDatabase.SaveAssets();
        }

        GUILayout.Space(10.0f);
    }

    /// <summary>
    /// 作成済みAssetの各項目を、生成時の表示ラベルで描画します。
    /// </summary>
    /// <param name="f_serializedObject">対象SerializedObject</param>
    private void DrawCreatedAssetSerializedFields(SerializedObject f_serializedObject)
    {
        DrawCreatedAssetProperty(
            f_serializedObject,
            "newVector2Field01",
            "newVector2Field01");
        GUILayout.Space(4.0f);

        DrawCreatedAssetProperty(
            f_serializedObject,
            "newVector2IntField02",
            "newVector2IntField02");
        GUILayout.Space(4.0f);

        DrawCreatedAssetProperty(
            f_serializedObject,
            "newField03",
            "newField03");
        GUILayout.Space(4.0f);

        DrawCreatedAssetProperty(
            f_serializedObject,
            "newListField04",
            "newListField04");
        GUILayout.Space(4.0f);

        DrawCreatedAssetProperty(
            f_serializedObject,
            "newListField05",
            "newListField05");
        GUILayout.Space(4.0f);

    }

    /// <summary>
    /// 指定したPropertyを表示ラベル付きで描画します。
    /// </summary>
    /// <param name="f_serializedObject">対象SerializedObject</param>
    /// <param name="f_propertyName">Property名</param>
    /// <param name="f_labelName">表示ラベル</param>
    private void DrawCreatedAssetProperty(
        SerializedObject f_serializedObject,
        string f_propertyName,
        string f_labelName)
    {
        SerializedProperty property = f_serializedObject.FindProperty(f_propertyName);

        if (property == null)
        {
            return;
        }

        EditorGUILayout.PropertyField(
            property,
            new GUIContent(f_labelName),
            true);
    }

    /// <summary>
    /// MinMaxFieldを表示ラベル付きで描画します。
    /// </summary>
    /// <param name="f_labelName">表示ラベル</param>
    /// <param name="f_minProperty">Min側Property</param>
    /// <param name="f_maxProperty">Max側Property</param>
    private void DrawCreatedAssetMinMaxField(
        string f_labelName,
        SerializedProperty f_minProperty,
        SerializedProperty f_maxProperty)
    {
        if (f_minProperty == null || f_maxProperty == null)
        {
            return;
        }

        Rect rowRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);

        float mainLabelWidth = 120.0f;
        float smallLabelWidth = 28.0f;
        float spacing = 6.0f;

        float valueAreaX = rowRect.x + mainLabelWidth + spacing;
        float valueAreaWidth = rowRect.width - mainLabelWidth - spacing;
        float fieldWidth = (valueAreaWidth - smallLabelWidth - smallLabelWidth - (spacing * 3.0f)) * 0.5f;
        fieldWidth = Mathf.Max(35.0f, fieldWidth);

        Rect labelRect = new Rect(rowRect.x, rowRect.y, mainLabelWidth, rowRect.height);
        Rect minLabelRect = new Rect(valueAreaX, rowRect.y, smallLabelWidth, rowRect.height);
        Rect minValueRect = new Rect(minLabelRect.xMax + spacing, rowRect.y, fieldWidth, rowRect.height);
        Rect maxLabelRect = new Rect(minValueRect.xMax + spacing, rowRect.y, smallLabelWidth, rowRect.height);
        Rect maxValueRect = new Rect(maxLabelRect.xMax + spacing, rowRect.y, fieldWidth, rowRect.height);

        EditorGUI.LabelField(labelRect, f_labelName);
        EditorGUI.LabelField(minLabelRect, "Min");
        EditorGUI.PropertyField(minValueRect, f_minProperty, GUIContent.none);
        EditorGUI.LabelField(maxLabelRect, "Max");
        EditorGUI.PropertyField(maxValueRect, f_maxProperty, GUIContent.none);
    }

    /// <summary>
    /// AssetをProject上で選択します。
    /// </summary>
    /// <param name="f_asset">選択するAsset</param>
    private void SelectAsset(CSS_NewToolDatTestDes f_asset)
    {
        Selection.activeObject = f_asset;
        EditorGUIUtility.PingObject(f_asset);
    }
}

#endif
