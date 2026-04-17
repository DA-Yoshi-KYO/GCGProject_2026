using UnityEngine;
using UnityEditor;

/*==================================
 * 作者   : 吉本竜
 * 内容   : エディタの基底クラス (コピペして使えるように) 
 * 履歴　 : 2026/04/17 新規作成(吉本竜)
 ================================*/

/// <summary>
/// ツール用のEditorWindow作成ベース
/// </summary>
public class Editor_Base : EditorWindow
{
    /// <summary>
    /// menuからウィンドウを開く。
    /// </summary>
    [MenuItem("Tools/ScriptableObjects/Test")]
    public static void ShowWindow()
    {
        GetWindow<Editor_Base>("Test");
    }

    /// <summary>
    /// ウィンドウが有効になったタイミングで呼ばれる。
    /// </summary>
    private void OnEnable()
    {
        // 必要になったら初期化
    }

    /// <summary>
    /// GUIを描画する（IMGUI）。
    /// </summary>
    private void OnGUI()
    {

    }
}
