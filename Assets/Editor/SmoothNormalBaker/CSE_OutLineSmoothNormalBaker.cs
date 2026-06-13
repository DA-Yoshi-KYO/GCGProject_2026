#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class OutlineSmoothNormalBaker : EditorWindow
{
    [MenuItem("Tools/Bake Smooth Normals To Tangent")]
    static void BakeSelected()
    {
        var go = Selection.activeGameObject;
        if (go == null)
        {
            EditorUtility.DisplayDialog("Error", "GameObjectを選択してください。", "OK");
            return;
        }

        string dir = "Assets/SmoothNormals";
        if (!System.IO.Directory.Exists(dir))
        {
            System.IO.Directory.CreateDirectory(dir);
            AssetDatabase.Refresh();
        }

        int count = 0;

        foreach (var smr in go.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            if (smr.sharedMesh == null) continue;
            Mesh bakedMesh = BakeAndSave(smr.sharedMesh, dir);
            if (bakedMesh != null)
            {
                Undo.RecordObject(smr, "Bake Smooth Normals");
                smr.sharedMesh = bakedMesh;
                EditorUtility.SetDirty(smr);
                count++;
                Debug.Log($"[Baker] {smr.gameObject.name} → {bakedMesh.name}");
            }
        }

        foreach (var mf in go.GetComponentsInChildren<MeshFilter>())
        {
            if (mf.sharedMesh == null) continue;
            Mesh bakedMesh = BakeAndSave(mf.sharedMesh, dir);
            if (bakedMesh != null)
            {
                Undo.RecordObject(mf, "Bake Smooth Normals");
                mf.sharedMesh = bakedMesh;
                EditorUtility.SetDirty(mf);
                count++;
                Debug.Log($"[Baker] {mf.gameObject.name} → {bakedMesh.name}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();

        EditorUtility.DisplayDialog("完了",
            $"{count} 個のメッシュにスムース法線をベイクしました。", "OK");
    }

    static Mesh BakeAndSave(Mesh src, string dir)
    {
        string safeName = SanitizeFileName(src.name);
        string path = $"{dir}/{safeName}_SN.asset";

        // 既存アセットは削除して再ベイク（古いデータが残らないように）
        var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (existing != null)
        {
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.Refresh();
        }

        Vector3[] normals = src.normals;
        if (normals == null || normals.Length == 0)
        {
            Debug.LogWarning($"[Baker] {src.name} に法線なし → スキップ");
            return null;
        }

        Vector3[] vertices = src.vertices;

        // ── Step1: 同位置頂点の法線を合算 ────────────────────────
        var posToNormal = new Dictionary<Vector3, Vector3>();
        for (int i = 0 ; i < vertices.Length ; i++)
        {
            if (!posToNormal.ContainsKey(vertices[i]))
                posToNormal[vertices[i]] = Vector3.zero;
            posToNormal[vertices[i]] += normals[i];
        }

        // ── Step2: スムース法線を計算し「元法線と同じ向き」に揃える ──
        // マントのように法線が内向きのメッシュでも
        // dot(元法線, スムース法線) < 0 なら反転して外向きに修正する
        var smoothNormals = new Vector3[vertices.Length];
        for (int i = 0 ; i < vertices.Length ; i++)
        {
            Vector3 smooth = posToNormal[vertices[i]].normalized;
            Vector3 orig = normals[i].normalized;

            // 元法線と逆向きになっていたら反転
            if (Vector3.Dot(orig, smooth) < 0f)
                smooth = -smooth;

            smoothNormals[i] = smooth;
        }

        // ── Step3: Tangentに書き込み ──────────────────────────────
        var tangents = new Vector4[vertices.Length];
        for (int i = 0 ; i < vertices.Length ; i++)
            tangents[i] = new Vector4(
                smoothNormals[i].x,
                smoothNormals[i].y,
                smoothNormals[i].z,
                1f);

        // ── Step4: new Mesh() で完全コピー（FBX読み取り専用回避）──
        var newMesh = new Mesh();
        newMesh.name = safeName + "_SN";
        newMesh.indexFormat = src.indexFormat;
        newMesh.vertices = src.vertices;
        newMesh.normals = src.normals;
        newMesh.uv = src.uv;
        newMesh.uv2 = src.uv2;
        newMesh.colors = src.colors;
        newMesh.boneWeights = src.boneWeights;
        newMesh.bindposes = src.bindposes;
        newMesh.subMeshCount = src.subMeshCount;

        for (int s = 0 ; s < src.subMeshCount ; s++)
            newMesh.SetTriangles(src.GetTriangles(s), s);

        for (int b = 0 ; b < src.blendShapeCount ; b++)
        {
            string bsName = src.GetBlendShapeName(b);
            int frameCount = src.GetBlendShapeFrameCount(b);
            for (int f = 0 ; f < frameCount ; f++)
            {
                float weight = src.GetBlendShapeFrameWeight(b, f);
                Vector3[] dv = new Vector3[src.vertexCount];
                Vector3[] dn = new Vector3[src.vertexCount];
                Vector3[] dt = new Vector3[src.vertexCount];
                src.GetBlendShapeFrameVertices(b, f, dv, dn, dt);
                newMesh.AddBlendShapeFrame(bsName, weight, dv, dn, dt);
            }
        }

        newMesh.tangents = tangents;
        newMesh.RecalculateBounds();

        AssetDatabase.CreateAsset(newMesh, path);
        Debug.Log($"[Baker] 保存: {path}");
        return newMesh;
    }

    static string SanitizeFileName(string name)
    {
        string result = name;
        foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            result = result.Replace(c, '_');
        result = result.Replace(':', '_');
        result = result.Replace('/', '_');
        result = result.Replace('\\', '_');
        return result;
    }
}
#endif
