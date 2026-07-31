using UnityEngine;

/*==================================================
 *  ファイル名  : CS_ActiveChildrenWithCamera.cs
 *  制作者      : 吉本 竜
 *  内容        : Cameraコンポーネントの有効状態に合わせて、
 *                自身の直下にある子オブジェクトを切り替える
 *  履歴        : 2026/07/31 新規作成
 *==================================================*/

/// <summary>
/// Cameraコンポーネントの有効状態に合わせて、
/// このオブジェクトの直下にある子を有効・無効にします。
/// </summary>
public class CS_ActiveChildrenWithCamera : MonoBehaviour
{
    [Header("状態を監視するCamera")]
    [SerializeField]
    private Camera cam_TargetCamera;

    private bool b_PreviousCameraEnabled;

    private void Start()
    {
        if (cam_TargetCamera == null)
        {
            Debug.LogWarning(
                $"{nameof(CS_ActiveChildrenWithCamera)}: Cameraが設定されていません。",
                this);

            return;
        }

        b_PreviousCameraEnabled = cam_TargetCamera.enabled;

        SetDirectChildrenActive(b_PreviousCameraEnabled);
    }

    private void Update()
    {
        if (cam_TargetCamera == null)
        {
            return;
        }

        bool b_IsCameraEnabled = cam_TargetCamera.enabled;

        // Cameraの状態が変わったときだけ子の状態を更新
        if (b_PreviousCameraEnabled == b_IsCameraEnabled)
        {
            return;
        }

        b_PreviousCameraEnabled = b_IsCameraEnabled;

        SetDirectChildrenActive(b_IsCameraEnabled);
    }

    /// <summary>
    /// 自身の直下にある子オブジェクトだけを切り替えます。
    /// 孫以降は直接調べません。
    /// </summary>
    private void SetDirectChildrenActive(bool b_IsActive)
    {
        for (int i = 0 ; i < transform.childCount ; i++)
        {
            Transform tf_Child = transform.GetChild(i);

            tf_Child.gameObject.SetActive(b_IsActive);
        }
    }
}
