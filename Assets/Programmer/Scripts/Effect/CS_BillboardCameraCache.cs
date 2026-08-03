using System.Collections.Generic;
using UnityEngine;

/*==================================================
 *  ファイル名  : CS_BillboardCameraCache.cs
 *  制作者      : 吉本 竜
 *  内容        : Display 1用MainCameraをキャッシュし、
 *                Billboardが参照するCameraを管理する
 *  履歴        : 2026/07/31 新規作成
 *==================================================*/

/// <summary>
/// Display 1へ描画するMainCameraをキャッシュし、
/// 現在有効なCameraをBillboardへ提供します。
/// </summary>
public static class CS_BillboardCameraCache
{
    /// <summary>
    /// Cameraと、そのCameraが所属するRootのキャッシュデータです。
    /// </summary>
    private sealed class CS_MainCameraCacheData
    {
        public Camera cam_MainCamera;
        public Transform tr_CameraRoot;
    }

    /// <summary>
    /// Display 1用MainCamera一覧です。
    /// </summary>
    private static readonly List<CS_MainCameraCacheData> list_MainCameraCache =
        new List<CS_MainCameraCacheData>();

    /// <summary>
    /// 現在Billboardが参照するCameraです。
    /// </summary>
    private static Camera cam_ActiveMainCamera;

    /// <summary>
    /// 現在有効なMainCameraのRootです。
    /// </summary>
    private static Transform tr_ActiveMainCameraRoot;

    private static int n_LastRefreshFrame = -1;

    /// <summary>
    /// Play開始時にStaticデータを初期化します。
    /// </summary>
    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticData()
    {
        list_MainCameraCache.Clear();

        cam_ActiveMainCamera = null;
        tr_ActiveMainCameraRoot = null;
        n_LastRefreshFrame = -1;
    }

    /// <summary>
    /// Scene内に存在するDisplay 1用MainCameraをすべて取得します。
    /// Cameraが非アクティブでも取得対象に含めます。
    /// 部屋生成後に呼び出してください。
    /// </summary>
    public static void CacheMainCameras()
    {
        list_MainCameraCache.Clear();

        Camera[] cam_AllCameras =
            Object.FindObjectsByType<Camera>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        for (int i = 0 ; i < cam_AllCameras.Length ; i++)
        {
            Camera cam_TargetCamera = cam_AllCameras[i];

            if (cam_TargetCamera == null)
            {
                continue;
            }

            // MainCameraタグだけを対象にします。
            if (!cam_TargetCamera.CompareTag("MainCamera"))
            {
                continue;
            }

            // targetDisplayの0がDisplay 1です。
            if (cam_TargetCamera.targetDisplay != 0)
            {
                continue;
            }

            CS_MainCameraCacheData cs_CacheData =
                new CS_MainCameraCacheData();

            cs_CacheData.cam_MainCamera = cam_TargetCamera;
            cs_CacheData.tr_CameraRoot =
                cam_TargetCamera.transform.root;

            list_MainCameraCache.Add(cs_CacheData);
        }

        RefreshActiveMainCamera();

        Debug.Log(
            "[BillboardCameraCache] Display 1用MainCameraを取得しました。"
            + " 取得数 : "
            + list_MainCameraCache.Count);
    }

    /// <summary>
    /// キャッシュ済みCameraから、
    /// Cameraコンポーネントが有効なCameraを選択します。
    /// Camera切り替え処理の最後に呼び出してください。
    /// </summary>
    public static void RefreshActiveMainCamera()
    {
        n_LastRefreshFrame = Time.frameCount;
        cam_ActiveMainCamera = null;
        tr_ActiveMainCameraRoot = null;

        float f_HighestDepth = float.NegativeInfinity;

        for (int i = 0 ; i < list_MainCameraCache.Count ; i++)
        {
            CS_MainCameraCacheData cs_CacheData =
                list_MainCameraCache[i];

            if (cs_CacheData == null)
            {
                continue;
            }

            Camera cam_TargetCamera =
                cs_CacheData.cam_MainCamera;

            if (cam_TargetCamera == null)
            {
                continue;
            }

            if (!cam_TargetCamera.CompareTag("MainCamera"))
            {
                continue;
            }

            // Display 1だけを対象にします。
            if (cam_TargetCamera.targetDisplay != 0)
            {
                continue;
            }

            // GameObjectのActive状態は確認しません。
            // Cameraコンポーネントのenabledだけを確認します。
            // 非アクティブな部屋のCameraを除外します。
            if (!cam_TargetCamera.isActiveAndEnabled)
            {
                continue;
            }

            // 複数有効だった場合はDepthが最も高いCameraを優先します。
            if (cam_TargetCamera.depth < f_HighestDepth)
            {
                continue;
            }

            f_HighestDepth = cam_TargetCamera.depth;

            cam_ActiveMainCamera = cam_TargetCamera;
            tr_ActiveMainCameraRoot =
                cs_CacheData.tr_CameraRoot;
        }
    }

    /// <summary>
    /// 1フレームに1回、有効なMainCameraを更新します。
    /// </summary>
    private static void RefreshActiveMainCameraIfNeeded()
    {
        if (n_LastRefreshFrame == Time.frameCount)
        {
            return;
        }

        if (list_MainCameraCache.Count <= 0)
        {
            CacheMainCameras();
            return;
        }

        RefreshActiveMainCamera();
    }

    public static Transform GetActiveMainCameraTransform()
    {
        RefreshActiveMainCameraIfNeeded();

        if (cam_ActiveMainCamera == null)
        {
            return null;
        }

        return cam_ActiveMainCamera.transform;
    }

    public static Transform GetActiveMainCameraRoot()
    {
        RefreshActiveMainCameraIfNeeded();
        return tr_ActiveMainCameraRoot;
    }

    public static Camera GetActiveMainCamera()
    {
        RefreshActiveMainCameraIfNeeded();
        return cam_ActiveMainCamera;
    }
}
