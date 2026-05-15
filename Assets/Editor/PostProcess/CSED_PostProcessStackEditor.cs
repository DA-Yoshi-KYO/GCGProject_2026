#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor(typeof(CSO_PostProcessStack))]
public class CSED_PostProcessStackEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var effectsProp = serializedObject.FindProperty("effects");

        // ✅ CSV_PostProcessVolumeBaseの派生クラスを全部列挙
        var types = System.AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(CSV_PostProcessVolumeBase)))
            .ToList();

        var typeLabels = types.Select(t => t.Name).ToArray();
        var typeFullNames = types.Select(t => t.AssemblyQualifiedName).ToArray();

        for (int i = 0 ; i < effectsProp.arraySize ; i++)
        {
            var entryProp = effectsProp.GetArrayElementAtIndex(i);
            var materialProp = entryProp.FindPropertyRelative("material");
            var typeNameProp = entryProp.FindPropertyRelative("volumeComponentTypeName");

            EditorGUILayout.BeginVertical("box");

            // マテリアル
            EditorGUILayout.PropertyField(materialProp, new GUIContent("Material"));

            // ドロップダウン
            int currentIndex = System.Array.IndexOf(typeFullNames, typeNameProp.stringValue);
            int selectedIndex = EditorGUILayout.Popup("Volume Component", currentIndex, typeLabels);
            if (selectedIndex >= 0)
                typeNameProp.stringValue = typeFullNames[selectedIndex];

            // 削除ボタン
            if (GUILayout.Button("Remove"))
                effectsProp.DeleteArrayElementAtIndex(i);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        // 追加ボタン
        if (GUILayout.Button("Add Effect"))
            effectsProp.InsertArrayElementAtIndex(effectsProp.arraySize);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
