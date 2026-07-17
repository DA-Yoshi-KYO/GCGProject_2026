using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public struct CSST_GimmickDirectionIndicatorSettings
{
    public Vector2 Size;
    public float MoveSpeed;
    public float Lifetime;
    public float SpawnInterval;
    public float FadeInSpeed;
    public float FadeOutSpeed;
    public float MaxAlpha;
    public float SearchTargetMoveSpeedMultiplier;
    public float SearchTargetSpawnIntervalMultiplier;

    public CSST_GimmickDirectionIndicatorSettings(
        Vector2 size,
        float moveSpeed,
        float lifetime,
        float spawnInterval,
        float fadeInSpeed,
        float fadeOutSpeed,
        float maxAlpha,
        float searchTargetMoveSpeedMultiplier,
        float searchTargetSpawnIntervalMultiplier)
    {
        Size = size;
        MoveSpeed = moveSpeed;
        Lifetime = lifetime;
        SpawnInterval = spawnInterval;
        FadeInSpeed = fadeInSpeed;
        FadeOutSpeed = fadeOutSpeed;
        MaxAlpha = maxAlpha;
        SearchTargetMoveSpeedMultiplier =
            searchTargetMoveSpeedMultiplier;
        SearchTargetSpawnIntervalMultiplier =
            searchTargetSpawnIntervalMultiplier;
    }
}

public sealed class CS_GimmickDirectionIndicatorRenderer
{
    private const float MinimumValue = 0.01f;
    private const float IndicatorHeightOffset = -2f;

    private static readonly int BaseColorProperty =
        Shader.PropertyToID("_BaseColor");
    private static readonly int ColorProperty =
        Shader.PropertyToID("_Color");

    private sealed class DirectionEmitter
    {
        public readonly GimmickBase Gimmick;
        public readonly MeshRenderer[] SourceRenderers;
        public readonly List<DirectionTriangle> ActiveTriangles =
            new List<DirectionTriangle>();

        public GimmickDirection Direction;
        public Vector3 PositionOffset;
        public bool HasSearchTarget;
        public float SpawnTimer;

        public DirectionEmitter(
            GimmickBase gimmick,
            GimmickDirection direction,
            Vector3 positionOffset,
            bool hasSearchTarget)
        {
            Gimmick = gimmick;
            Direction = direction;
            PositionOffset = positionOffset;
            HasSearchTarget = hasSearchTarget;
            SpawnTimer = 0.0f;
            SourceRenderers =
                gimmick.GetComponentsInChildren<MeshRenderer>(true);
        }
    }

    private sealed class DirectionTriangle
    {
        public readonly GameObject GameObject;
        public readonly Transform Transform;
        public readonly MeshRenderer Renderer;
        public readonly MaterialPropertyBlock PropertyBlock;

        public Vector3 MoveDirection;
        public float Age;

        public DirectionTriangle(
            GameObject gameObject,
            MeshRenderer renderer)
        {
            GameObject = gameObject;
            Transform = gameObject.transform;
            Renderer = renderer;
            PropertyBlock = new MaterialPropertyBlock();
        }
    }

    private readonly Dictionary<GimmickBase, DirectionEmitter> emitters =
        new Dictionary<GimmickBase, DirectionEmitter>();
    private readonly List<GimmickBase> removeTargets =
        new List<GimmickBase>();
    private readonly Stack<DirectionTriangle> trianglePool =
        new Stack<DirectionTriangle>();

    private Transform indicatorRoot;
    private Mesh triangleMesh;
    private Material triangleMaterial;
    private bool isDisposed;
    private bool hasLoggedShaderWarning;

    public void UpdateIndicators(
        IReadOnlyDictionary<GimmickBase, GimmickDirection> directions,
        IReadOnlyDictionary<GimmickBase, Vector3> positionOffsets,
        IReadOnlyDictionary<GimmickBase, bool> searchTargetStates,
        CSST_GimmickDirectionIndicatorSettings settings,
        float deltaTime)
    {
        if (isDisposed)
            return;

        if (!EnsureResources())
            return;

        RemoveInactiveEmitters(directions);

        foreach (KeyValuePair<GimmickBase, GimmickDirection> pair
                 in directions)
        {
            GimmickBase gimmick = pair.Key;
            if (gimmick == null)
                continue;

            Vector3 positionOffset =
                positionOffsets.TryGetValue(
                    gimmick,
                    out Vector3 configuredOffset)
                    ? configuredOffset
                    : Vector3.zero;
            bool hasSearchTarget =
                searchTargetStates.TryGetValue(
                    gimmick,
                    out bool configuredSearchTargetState) &&
                configuredSearchTargetState;

            if (!emitters.TryGetValue(
                    gimmick,
                    out DirectionEmitter emitter))
            {
                emitter =
                    new DirectionEmitter(
                        gimmick,
                        pair.Value,
                        positionOffset,
                        hasSearchTarget);
                emitters.Add(gimmick, emitter);
            }
            else if (emitter.Direction != pair.Value ||
                     (emitter.PositionOffset - positionOffset)
                     .sqrMagnitude > 0.0001f)
            {
                ReleaseEmitterTriangles(emitter);
                emitter.Direction = pair.Value;
                emitter.PositionOffset = positionOffset;
                emitter.HasSearchTarget = hasSearchTarget;
                emitter.SpawnTimer = 0.0f;
            }
            else if (emitter.HasSearchTarget != hasSearchTarget)
            {
                emitter.HasSearchTarget = hasSearchTarget;
                emitter.SpawnTimer = 0.0f;
            }
        }

        foreach (DirectionEmitter emitter in emitters.Values)
        {
            CSST_GimmickDirectionIndicatorSettings
                effectiveSettings = settings;
            if (emitter.HasSearchTarget)
            {
                effectiveSettings.MoveSpeed *=
                    Mathf.Max(
                        settings.SearchTargetMoveSpeedMultiplier,
                        1.0f);
                effectiveSettings.SpawnInterval *=
                    Mathf.Clamp(
                        settings.SearchTargetSpawnIntervalMultiplier,
                        MinimumValue,
                        1.0f);
            }

            UpdateEmitter(
                emitter,
                effectiveSettings,
                Mathf.Max(deltaTime, 0.0f));
        }
    }

    public void ClearIndicators()
    {
        foreach (DirectionEmitter emitter in emitters.Values)
        {
            ReleaseEmitterTriangles(emitter);
        }

        emitters.Clear();
        removeTargets.Clear();
    }

    public void Dispose()
    {
        if (isDisposed)
            return;

        isDisposed = true;
        emitters.Clear();
        removeTargets.Clear();
        trianglePool.Clear();

        if (indicatorRoot != null)
        {
            Object.Destroy(indicatorRoot.gameObject);
            indicatorRoot = null;
        }

        if (triangleMesh != null)
        {
            Object.Destroy(triangleMesh);
            triangleMesh = null;
        }

        if (triangleMaterial != null)
        {
            Object.Destroy(triangleMaterial);
            triangleMaterial = null;
        }
    }

    private bool EnsureResources()
    {
        if (indicatorRoot == null)
        {
            GameObject rootObject =
                new GameObject("GimmickDirectionIndicatorRoot");
            indicatorRoot = rootObject.transform;
        }

        if (triangleMesh == null)
        {
            triangleMesh = CreateTriangleMesh();
        }

        if (triangleMaterial != null)
            return true;

        Shader shader =
            Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        if (shader == null)
        {
            if (!hasLoggedShaderWarning)
            {
                hasLoggedShaderWarning = true;
                Debug.LogWarning(
                    "ギミック方向表示に使用できるShaderが見つかりません。");
            }

            return false;
        }

        triangleMaterial = new Material(shader)
        {
            name = "GimmickDirectionIndicatorMaterial",
            hideFlags = HideFlags.DontSave,
            renderQueue = (int)RenderQueue.Transparent,
        };

        triangleMaterial.SetOverrideTag(
            "RenderType",
            "Transparent");
        SetMaterialFloatIfAvailable(
            triangleMaterial,
            "_Surface",
            1.0f);
        SetMaterialFloatIfAvailable(
            triangleMaterial,
            "_SrcBlend",
            (float)BlendMode.SrcAlpha);
        SetMaterialFloatIfAvailable(
            triangleMaterial,
            "_DstBlend",
            (float)BlendMode.OneMinusSrcAlpha);
        SetMaterialFloatIfAvailable(
            triangleMaterial,
            "_ZWrite",
            0.0f);
        SetMaterialFloatIfAvailable(
            triangleMaterial,
            "_Cull",
            (float)CullMode.Off);
        triangleMaterial.EnableKeyword(
            "_SURFACE_TYPE_TRANSPARENT");

        Color green = new Color(0.0f, 1.0f, 0.0f, 0.0f);
        if (triangleMaterial.HasProperty(BaseColorProperty))
        {
            triangleMaterial.SetColor(
                BaseColorProperty,
                green);
        }
        if (triangleMaterial.HasProperty(ColorProperty))
        {
            triangleMaterial.SetColor(
                ColorProperty,
                green);
        }

        return true;
    }

    private void RemoveInactiveEmitters(
        IReadOnlyDictionary<GimmickBase, GimmickDirection> directions)
    {
        removeTargets.Clear();

        foreach (KeyValuePair<GimmickBase, DirectionEmitter> pair
                 in emitters)
        {
            if (pair.Key == null ||
                !directions.ContainsKey(pair.Key))
            {
                removeTargets.Add(pair.Key);
            }
        }

        foreach (GimmickBase gimmick in removeTargets)
        {
            if (!emitters.TryGetValue(
                    gimmick,
                    out DirectionEmitter emitter))
            {
                continue;
            }

            ReleaseEmitterTriangles(emitter);
            emitters.Remove(gimmick);
        }
    }

    private void UpdateEmitter(
        DirectionEmitter emitter,
        CSST_GimmickDirectionIndicatorSettings settings,
        float deltaTime)
    {
        float lifetime =
            Mathf.Max(settings.Lifetime, MinimumValue);
        float fadeInSpeed =
            Mathf.Max(settings.FadeInSpeed, MinimumValue);
        float fadeOutSpeed =
            Mathf.Max(settings.FadeOutSpeed, MinimumValue);
        float maxAlpha =
            Mathf.Clamp01(settings.MaxAlpha);

        for (int i = emitter.ActiveTriangles.Count - 1;
             i >= 0;
             i--)
        {
            DirectionTriangle triangle =
                emitter.ActiveTriangles[i];
            triangle.Age += deltaTime;

            if (triangle.Age >= lifetime)
            {
                emitter.ActiveTriangles.RemoveAt(i);
                ReleaseTriangle(triangle);
                continue;
            }

            triangle.Transform.position +=
                triangle.MoveDirection *
                Mathf.Max(settings.MoveSpeed, 0.0f) *
                deltaTime;

            float remainingTime =
                lifetime - triangle.Age;
            float fadeInAlpha =
                triangle.Age * fadeInSpeed;
            float fadeOutAlpha =
                remainingTime * fadeOutSpeed;
            float alpha = Mathf.Min(
                maxAlpha,
                fadeInAlpha,
                fadeOutAlpha);

            SetTriangleAlpha(triangle, alpha);
        }

        emitter.SpawnTimer -= deltaTime;
        if (emitter.SpawnTimer > 0.0f)
            return;

        SpawnTriangle(emitter, settings);
        emitter.SpawnTimer =
            Mathf.Max(settings.SpawnInterval, MinimumValue);
    }

    private void SpawnTriangle(
        DirectionEmitter emitter,
        CSST_GimmickDirectionIndicatorSettings settings)
    {
        Vector3 moveDirection =
            GetWorldDirection(emitter.Direction);
        if (moveDirection.sqrMagnitude <= 0.0001f)
            return;

        moveDirection.Normalize();

        DirectionTriangle triangle =
            GetTriangle();
        triangle.Age = 0.0f;
        triangle.MoveDirection = moveDirection;

        triangle.Transform.SetPositionAndRotation(
            GetIndicatorOrigin(emitter),
            Quaternion.LookRotation(
                moveDirection,
                Vector3.up));
        triangle.Transform.localScale =
            new Vector3(
                Mathf.Max(settings.Size.x, MinimumValue),
                1.0f,
                Mathf.Max(settings.Size.y, MinimumValue));

        SetTriangleAlpha(triangle, 0.0f);
        emitter.ActiveTriangles.Add(triangle);
    }

    private DirectionTriangle GetTriangle()
    {
        if (trianglePool.Count > 0)
        {
            DirectionTriangle pooledTriangle =
                trianglePool.Pop();
            pooledTriangle.GameObject.SetActive(true);
            return pooledTriangle;
        }

        GameObject triangleObject =
            new GameObject("GimmickDirectionTriangle");
        triangleObject.transform.SetParent(
            indicatorRoot,
            false);

        MeshFilter meshFilter =
            triangleObject.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = triangleMesh;

        MeshRenderer meshRenderer =
            triangleObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = triangleMaterial;
        meshRenderer.shadowCastingMode =
            ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        meshRenderer.lightProbeUsage =
            LightProbeUsage.Off;
        meshRenderer.reflectionProbeUsage =
            ReflectionProbeUsage.Off;

        return new DirectionTriangle(
            triangleObject,
            meshRenderer);
    }

    private void ReleaseEmitterTriangles(
        DirectionEmitter emitter)
    {
        foreach (DirectionTriangle triangle
                 in emitter.ActiveTriangles)
        {
            ReleaseTriangle(triangle);
        }

        emitter.ActiveTriangles.Clear();
    }

    private void ReleaseTriangle(
        DirectionTriangle triangle)
    {
        triangle.GameObject.SetActive(false);
        trianglePool.Push(triangle);
    }

    private static Vector3 GetIndicatorOrigin(
        DirectionEmitter emitter)
    {
        Vector3 origin =
            emitter.Gimmick.transform.position;
        float highestPoint =
            origin.y + IndicatorHeightOffset;
        bool hasVisibleRenderer = false;

        foreach (MeshRenderer renderer
                 in emitter.SourceRenderers)
        {
            if (renderer == null ||
                !renderer.enabled ||
                !renderer.gameObject.activeInHierarchy)
            {
                continue;
            }

            highestPoint = hasVisibleRenderer
                ? Mathf.Max(highestPoint, renderer.bounds.max.y)
                : renderer.bounds.max.y;
            hasVisibleRenderer = true;
        }

        origin.y =
            highestPoint + IndicatorHeightOffset;
        origin +=
            emitter.Gimmick.transform.TransformVector(
                emitter.PositionOffset);
        return origin;
    }

    private static void SetTriangleAlpha(
        DirectionTriangle triangle,
        float alpha)
    {
        Color color =
            new Color(
                0.0f,
                1.0f,
                0.0f,
                Mathf.Clamp01(alpha));

        triangle.PropertyBlock.Clear();
        triangle.PropertyBlock.SetColor(
            BaseColorProperty,
            color);
        triangle.PropertyBlock.SetColor(
            ColorProperty,
            color);
        triangle.Renderer.SetPropertyBlock(
            triangle.PropertyBlock);
    }

    private static Vector3 GetWorldDirection(
        GimmickDirection direction)
    {
        switch (direction)
        {
            case GimmickDirection.Up:
                return Vector3.back;

            case GimmickDirection.Down:
                return Vector3.forward;

            case GimmickDirection.Left:
                return Vector3.right;

            case GimmickDirection.Right:
                return Vector3.left;

            default:
                return Vector3.zero;
        }
    }

    private static Mesh CreateTriangleMesh()
    {
        Mesh mesh = new Mesh
        {
            name = "GimmickDirectionTriangleMesh",
            hideFlags = HideFlags.DontSave,
        };

        mesh.vertices = new[]
        {
            new Vector3(-0.5f, 0.0f, -0.5f),
            new Vector3(0.0f, 0.0f, 0.5f),
            new Vector3(0.5f, 0.0f, -0.5f),
        };
        mesh.normals = new[]
        {
            Vector3.up,
            Vector3.up,
            Vector3.up,
        };
        mesh.uv = new[]
        {
            new Vector2(0.0f, 0.0f),
            new Vector2(0.5f, 1.0f),
            new Vector2(1.0f, 0.0f),
        };
        mesh.triangles = new[] { 0, 1, 2 };
        mesh.RecalculateBounds();

        return mesh;
    }

    private static void SetMaterialFloatIfAvailable(
        Material material,
        string propertyName,
        float value)
    {
        if (material.HasProperty(propertyName))
        {
            material.SetFloat(propertyName, value);
        }
    }
}
