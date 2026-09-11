/*
+=====================================
ファイル名 : CSE_processing_load_camera.cs
概要       : Processing Load用Camera・RenderTexture管理
作者       : ヨシモト リョウ
履歴       : 2026/09/11 新規作成
=====================================+
*/

#if UNITY_EDITOR

using UnityEngine;

public partial class CSE_processing_load
{
    // 複製元となるMainCamera
    private Camera sourceCamera;

    // Processing Load専用Camera
    private Camera targetCamera;

    // Processing Load専用CameraのGameObject
    private GameObject targetCameraObject;

    // Processing Load用RenderTexture
    private RenderTexture renderTexture;


    /// <summary>
    /// MainCameraを取得し、
    /// Processing Load専用Cameraとして複製する
    /// </summary>
    private void CreateProcessingCamera()
    {
        // すでにCameraを作成済みの場合は何もしない
        if (targetCamera != null)
        {
            return;
        }


        // MainCameraタグが付いている有効なCameraを取得
        sourceCamera =
            Camera.main;


        // MainCameraが存在しない場合
        if (sourceCamera == null)
        {
            return;
        }


        // MainCameraが持っているCamera設定や
        // Post Processing関連Componentも含めて複製するため、
        // Camera ComponentだけではなくGameObjectごと複製
        targetCameraObject =
            Instantiate(
                sourceCamera.gameObject
            );


        // Hierarchy上で識別しやすい名前に変更
        targetCameraObject.name =
            "ProcessingLoadCamera";


        // MainCameraタグが2つ存在する状態を防ぐ
        targetCameraObject.tag =
            "Untagged";


        // 複製したGameObjectからCamera Componentを取得
        targetCamera =
            targetCameraObject.GetComponent<Camera>();


        // Camera Componentが取得できなかった場合
        if (targetCamera == null)
        {
            // 複製したGameObjectを削除
            DestroyImmediate(
                targetCameraObject
            );

            // 参照を解除
            targetCameraObject =
                null;

            return;
        }


        // 通常のUnity Camera更新では描画しない
        // RenderProcessingCamera()から手動でRenderする
        targetCamera.enabled =
            false;


        // 複製元CameraがRenderTextureを持っていても
        // その設定は引き継がない
        targetCamera.targetTexture =
            null;


        // AudioListenerまで複製されると
        // AudioListenerが複数存在する警告が出るため無効化
        AudioListener audioListener =
            targetCameraObject.GetComponent<AudioListener>();


        if (audioListener != null)
        {
            audioListener.enabled =
                false;
        }
    }


    /// <summary>
    /// Processing Load用RenderTextureを作成する
    /// </summary>
    private void CreateRenderTexture(
        int width,
        int height)
    {
        // すでに同じサイズのRenderTextureが存在する場合
        // 毎フレーム作り直す必要がないためそのまま使用する
        if (renderTexture != null &&
            renderTexture.width == width &&
            renderTexture.height == height)
        {
            return;
        }


        // サイズが変化した場合は古いRenderTextureを削除
        DestroyRenderTexture();


        // GameViewと近い色表示になる形式でRenderTextureを作成
        renderTexture =
            new RenderTexture(
                width,
                height,
                24,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Default
            );


        // Debug時に識別しやすい名前を設定
        renderTexture.name =
            "ProcessingLoadRenderTexture";


        // GPU側にRenderTextureリソースを作成
        renderTexture.Create();
    }


    /// <summary>
    /// Processing Load専用CameraでRenderTextureへ描画する
    /// </summary>
    /// <returns>
    /// 描画結果のRenderTexture
    /// </returns>
    private RenderTexture RenderProcessingCamera(
        int width,
        int height)
    {
        // Processing Load専用Cameraが存在しない場合
        if (targetCamera == null)
        {
            return null;
        }


        // GameView解像度に合わせたRenderTextureを準備
        CreateRenderTexture(
            width,
            height
        );


        // RenderTexture作成に失敗した場合
        if (renderTexture == null)
        {
            return null;
        }


        // 元MainCameraの現在位置・回転を
        // Processing Load専用Cameraへ反映
        if (sourceCamera != null)
        {
            targetCamera.transform.position =
                sourceCamera.transform.position;


            targetCamera.transform.rotation =
                sourceCamera.transform.rotation;
        }


        // Processing Load専用Cameraの描画先を設定
        targetCamera.targetTexture =
            renderTexture;


        // Cameraを手動描画
        targetCamera.Render();


        // 描画結果を返す
        return renderTexture;
    }


    /// <summary>
    /// Processing Load用RenderTextureを削除する
    /// </summary>
    private void DestroyRenderTexture()
    {
        // RenderTextureが存在しない場合
        if (renderTexture == null)
        {
            return;
        }


        // Cameraが削除対象RenderTextureを参照している場合
        // 先に参照を解除する
        if (targetCamera != null &&
            targetCamera.targetTexture == renderTexture)
        {
            targetCamera.targetTexture =
                null;
        }


        // GPU側のRenderTextureリソースを解放
        renderTexture.Release();


        // Editor上のRenderTextureオブジェクトを削除
        DestroyImmediate(
            renderTexture
        );


        // 参照を解除
        renderTexture =
            null;
    }


    /// <summary>
    /// Processing Load用CameraとRenderTextureを削除する
    /// </summary>
    private void DestroyProcessingResources()
    {
        // RenderTexture削除
        DestroyRenderTexture();


        // Processing Load専用Cameraを削除
        if (targetCameraObject != null)
        {
            DestroyImmediate(
                targetCameraObject
            );
        }


        // 各参照を解除
        targetCamera =
            null;

        targetCameraObject =
            null;

        sourceCamera =
            null;
    }
}

#endif
