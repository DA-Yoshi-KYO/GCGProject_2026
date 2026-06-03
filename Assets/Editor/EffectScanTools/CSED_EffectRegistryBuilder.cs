/*
+=====================================
 ファイル名 : CSED_EffectRegistryBuilder.cs
 概要     : EffectPrefabフォルダをスキャンしてEffectRegistryとEffectId enumを自動生成する
 作者     : ヨシモト リョウ
 履歴     : 2026/06/03 新規作成
=====================================+
*/

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// EffectPrefabフォルダをスキャンして、RegistryとEffectId enumを自動生成するEditor用クラスです。
/// </summary>
public static class CSED_EffectRegistryBuilder
{
    private const string CSEffectFolderPath = "Assets/Effect/Prefab/CSEffect";
    private const string ParticleSystemEffectFolderPath = "Assets/Effect/Prefab/ParticleSystemEffect";
    private const string SpriteSheetEffectFolderPath = "Assets/Effect/Prefab/SpriteSheetEffect";

    private const string RegistryAssetPath = "Assets/Resources/Effect/CSS_EffectRegistry.asset";
    private const string GeneratedEnumPath = "Assets/Programmer/Scripts/Effect/Generated/CSE_EffectId.cs";

    /// <summary>
    /// EffectRegistryとCSE_EffectIdを再生成します。
    /// </summary>
    [MenuItem("Tools/Effect/Rebuild Effect Registry")]
    public static void RebuildEffectRegistry()
    {
        EnsureFolder("Assets/Effect");
        EnsureFolder("Assets/Programmer/Scripts/Effect");
        EnsureFolder("Assets/Programmer/Scripts/Effect/Generated");

        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/Resources/Effect");

        CSS_EffectRegistry effectRegistry = LoadOrCreateRegistry();

        if (effectRegistry == null)
        {
            Debug.LogError("[EffectRegistryBuilder] CSS_EffectRegistry の作成に失敗しました。");
            return;
        }

        effectRegistry.Clear();

        List<string> list_EffectIdName = new List<string>();
        HashSet<string> hash_UsedEffectIdName = new HashSet<string>();

        // Noneは必ず先頭に入れます。
        list_EffectIdName.Add("None");
        hash_UsedEffectIdName.Add("None");

        ScanEffectFolder(
            CSEffectFolderPath,
            CSE_EffectType.CustomCS,
            effectRegistry,
            list_EffectIdName,
            hash_UsedEffectIdName);

        ScanEffectFolder(
            ParticleSystemEffectFolderPath,
            CSE_EffectType.ParticleSystem,
            effectRegistry,
            list_EffectIdName,
            hash_UsedEffectIdName);

        ScanEffectFolder(
            SpriteSheetEffectFolderPath,
            CSE_EffectType.SpriteSheet,
            effectRegistry,
            list_EffectIdName,
            hash_UsedEffectIdName);

        EditorUtility.SetDirty(effectRegistry);
        AssetDatabase.SaveAssets();

        GenerateEffectIdEnum(list_EffectIdName);

        AssetDatabase.Refresh();

        Debug.Log("[EffectRegistryBuilder] EffectRegistry と CSE_EffectId を再生成しました。登録数 : " + (list_EffectIdName.Count - 1));
    }

    /// <summary>
    /// Registryアセットを読み込みます。存在しない場合は新規作成します。
    /// </summary>
    /// <returns>EffectRegistry。</returns>
    private static CSS_EffectRegistry LoadOrCreateRegistry()
    {
        CSS_EffectRegistry effectRegistry =
            AssetDatabase.LoadAssetAtPath<CSS_EffectRegistry>(RegistryAssetPath);

        if (effectRegistry != null)
        {
            return effectRegistry;
        }

        effectRegistry = ScriptableObject.CreateInstance<CSS_EffectRegistry>();

        AssetDatabase.CreateAsset(effectRegistry, RegistryAssetPath);
        AssetDatabase.SaveAssets();

        Debug.Log("[EffectRegistryBuilder] CSS_EffectRegistry を新規作成しました : " + RegistryAssetPath);

        return effectRegistry;
    }

    /// <summary>
    /// 指定フォルダ内のEffectPrefabをスキャンします。
    /// </summary>
    /// <param name="folderPath">検索対象フォルダ。</param>
    /// <param name="effectType">フォルダに対応するEffect種別。</param>
    /// <param name="effectRegistry">登録先Registry。</param>
    /// <param name="list_EffectIdName">生成するEffectId名一覧。</param>
    /// <param name="hash_UsedEffectIdName">重複確認用HashSet。</param>
    private static void ScanEffectFolder(
        string folderPath,
        CSE_EffectType effectType,
        CSS_EffectRegistry effectRegistry,
        List<string> list_EffectIdName,
        HashSet<string> hash_UsedEffectIdName)
    {
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogWarning("[EffectRegistryBuilder] フォルダが存在しません : " + folderPath);
            return;
        }

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });

        for (int i = 0 ; i < prefabGuids.Length ; i++)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);

            GameObject prefabObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefabObject == null)
            {
                continue;
            }

            CS_EffectRoot effectRoot = prefabObject.GetComponent<CS_EffectRoot>();

            if (effectRoot == null)
            {
                Debug.LogWarning("[EffectRegistryBuilder] CS_EffectRoot が無いため登録しません : " + prefabPath);
                continue;
            }

            string prefabName = Path.GetFileNameWithoutExtension(prefabPath);
            string effectIdName = CreateEffectIdName(effectType, prefabName);

            if (string.IsNullOrEmpty(effectIdName))
            {
                Debug.LogWarning("[EffectRegistryBuilder] EffectId名を作れないため登録しません : " + prefabPath);
                continue;
            }

            if (hash_UsedEffectIdName.Contains(effectIdName))
            {
                Debug.LogWarning("[EffectRegistryBuilder] EffectId名が重複しています。Prefab名を変更してください : " + effectIdName);
                continue;
            }

            hash_UsedEffectIdName.Add(effectIdName);
            list_EffectIdName.Add(effectIdName);

            CSS_EffectData effectData = new CSS_EffectData();

            // EffectNameには、CSE_EffectIdと一致する安全な名前を入れます。
            // これにより、後で CSE_EffectId.MagicCircle.ToString() で検索できます。
            effectData.SetData(effectIdName, effectType, effectRoot);

            effectRegistry.AddEffectData(effectData);

            Debug.Log("[EffectRegistryBuilder] 登録 : " + effectIdName + " / " + prefabPath);
        }
    }

    /// <summary>
    /// EffectTypeとPrefab名から、CSE_EffectId用の名前を作成します。
    /// </summary>
    /// <param name="effectType">エフェクト種別。</param>
    /// <param name="prefabName">Prefab名。</param>
    /// <returns>EffectId名。</returns>
    private static string CreateEffectIdName(CSE_EffectType effectType, string prefabName)
    {
        string safePrefabName = ConvertToSafeEnumName(prefabName);

        if (string.IsNullOrEmpty(safePrefabName))
        {
            return string.Empty;
        }

        return GetEffectTypePrefix(effectType) + "_" + safePrefabName;
    }

    /// <summary>
    /// EffectTypeからEffectId用の接頭辞を取得します。
    /// </summary>
    /// <param name="effectType">エフェクト種別。</param>
    /// <returns>EffectId用接頭辞。</returns>
    private static string GetEffectTypePrefix(CSE_EffectType effectType)
    {
        switch (effectType)
        {
            case CSE_EffectType.CustomCS:
                return "CustomCS";

            case CSE_EffectType.ParticleSystem:
                return "ParticleSystem";

            case CSE_EffectType.SpriteSheet:
                return "SpriteSheet";

            default:
                return "Unknown";
        }
    }

    /// <summary>
    /// CSE_EffectId.csを自動生成します。
    /// </summary>
    /// <param name="list_EffectIdName">EffectId名一覧。</param>
    private static void GenerateEffectIdEnum(List<string> list_EffectIdName)
    {
        StringBuilder stringBuilder = new StringBuilder();

        stringBuilder.AppendLine("/*");
        stringBuilder.AppendLine("+=====================================");
        stringBuilder.AppendLine(" ファイル名 : CSE_EffectId.cs");
        stringBuilder.AppendLine(" 概要     : EffectRegistryBuilderによって自動生成されるEffectId");
        stringBuilder.AppendLine(" 作者     : ヨシモト リョウ");
        stringBuilder.AppendLine(" 履歴     : 自動生成");
        stringBuilder.AppendLine("=====================================+");
        stringBuilder.AppendLine("*/");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("/// <summary>");
        stringBuilder.AppendLine("/// エフェクトPrefabを外部から安全に指定するためのIDです。");
        stringBuilder.AppendLine("/// このファイルは自動生成のため、手動編集しないでください。");
        stringBuilder.AppendLine("/// </summary>");
        stringBuilder.AppendLine("public enum CSE_EffectId");
        stringBuilder.AppendLine("{");

        for (int i = 0 ; i < list_EffectIdName.Count ; i++)
        {
            string effectIdName = list_EffectIdName[i];

            stringBuilder.Append("    ");
            stringBuilder.Append(effectIdName);

            if (i < list_EffectIdName.Count - 1)
            {
                stringBuilder.Append(",");
            }

            stringBuilder.AppendLine();
        }

        stringBuilder.AppendLine("}");

        File.WriteAllText(GeneratedEnumPath, stringBuilder.ToString(), Encoding.UTF8);

        Debug.Log("[EffectRegistryBuilder] CSE_EffectId.cs を生成しました : " + GeneratedEnumPath);
    }

    /// <summary>
    /// Prefab名をC#のenum名として安全な文字列に変換します。
    /// </summary>
    /// <param name="sourceName">変換元の名前。</param>
    /// <returns>enumに使える名前。</returns>
    private static string ConvertToSafeEnumName(string sourceName)
    {
        if (string.IsNullOrEmpty(sourceName))
        {
            return string.Empty;
        }

        string safeName = Regex.Replace(sourceName, "[^a-zA-Z0-9_]", "_");

        if (string.IsNullOrEmpty(safeName))
        {
            return string.Empty;
        }

        if (char.IsDigit(safeName[0]))
        {
            safeName = "Effect_" + safeName;
        }

        return safeName;
    }

    /// <summary>
    /// 指定フォルダが存在しない場合に作成します。
    /// </summary>
    /// <param name="folderPath">作成したいフォルダパス。</param>
    private static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
        {
            return;
        }

        string parentFolder = Path.GetDirectoryName(folderPath);
        string folderName = Path.GetFileName(folderPath);

        if (string.IsNullOrEmpty(parentFolder) || string.IsNullOrEmpty(folderName))
        {
            return;
        }

        parentFolder = parentFolder.Replace("\\", "/");

        EnsureFolder(parentFolder);

        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder(parentFolder, folderName);
        }
    }
}
