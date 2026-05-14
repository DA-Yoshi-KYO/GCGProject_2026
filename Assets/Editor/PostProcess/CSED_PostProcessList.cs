#if UNITY_EDITOR
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(CSV_PostProcessVolumeBase))]
public class CSED_PostProcessList : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("material"));

        // CustomVolumeComponentの派生クラスを全部列挙してドロップダウンに
        var types = System.AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsSubclassOf(typeof(CSV_PostProcessVolumeBase)) && !t.IsAbstract)
            .ToList();

        var typeNames = types.Select(t => t.Name).ToArray();
        var typeProp = serializedObject.FindProperty("volumeComponentTypeName");

        int currentIndex = System.Array.IndexOf(typeNames, typeProp.stringValue);
        int selectedIndex = EditorGUILayout.Popup("Volume Component", currentIndex, typeNames);

        if (selectedIndex >= 0)
            // アセンブリ修飾名で保存しておくとGetType()が確実に取れる
            typeProp.stringValue = types[selectedIndex].AssemblyQualifiedName;

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
