using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

// OptionのUIを管理するクラス
// 制作者　秋野

public class OptionSys : MonoBehaviour
{
    [SerializeField] private string MangerName = "GameOptionManager";

    [SerializeField] private GameObject UI;
    [SerializeField] private GameObject VolumeUI;
    [SerializeField] private GameObject KeyUI;
    [SerializeField] private Slider BGM;
    [SerializeField] private Slider SE;

    [SerializeField] private float animationSpeed = 5.0f;

    private GameObject _optionManager;
    private Option _option;

    private bool _isOptionUIActive = true;

    public bool IsVolumeUIActive { get; private set; } = false;
    public bool IsKeyUIActive { get; private set; } = false;

    private void Awake()
    {
        _optionManager = GameObject.Find(MangerName);

        if (_optionManager == null)
        {
            Debug.LogError("GameOptionManagerが見つかりません");
            return;
        }

        _option = _optionManager.GetComponent<Option>();

        if (_option == null)
        {
            Debug.LogError("Optionコンポーネントが見つかりません");
            return;
        }

        BGM.value = _option.GetBGMVolume();
        SE.value = _option.GetSEVolume();


        // 初期状態
        VolumeUI.SetActive(false);
        KeyUI.SetActive(false);

        IsVolumeUIActive = false;
        IsKeyUIActive = false;

        // アニメーション初期化
        UI.transform.localScale = Vector3.zero;
    }

    private void Update()
    {
        float delta = Time.unscaledDeltaTime;

        if (_isOptionUIActive)
        {
            // 開くアニメーション
            UI.transform.localScale = Vector3.Lerp
            (
                UI.transform.localScale,
                Vector3.one,
                animationSpeed * delta
            );

            // ほぼ1になったら固定
            if (Vector3.Distance(UI.transform.localScale, Vector3.one) < 0.01f)
            {
                UI.transform.localScale = Vector3.one;
            }

            // UIが表示完了するまで待つ
            if (UI.transform.localScale.x < 0.95f)
            {
                return;
            }

            // サブUI管理
            bool isSubUIActive = false;

            // VolumeUI
            if (IsVolumeUIActive)
            {
                VolumeUI.SetActive(true);
                isSubUIActive = true;
            }
            else
            {
                VolumeUI.SetActive(false);
            }

            // KeyUI
            if (IsKeyUIActive)
            {
                KeyUI.SetActive(true);
                isSubUIActive = true;
            }
            else
            {
                KeyUI.SetActive(false);
            }

            // メインUI表示切替
            UI.SetActive(!isSubUIActive);
        }
        else
        {
            // 閉じるアニメーション
            UI.transform.localScale = Vector3.Lerp
            (
                UI.transform.localScale,
                Vector3.zero,
                animationSpeed * delta
            );

            if (UI.transform.localScale.magnitude < 0.01f)
            {
                UI.transform.localScale = Vector3.zero;

                _option.SetIsOptionUIActive(false);

                Time.timeScale = 1f;

                Destroy(this.gameObject);
            }
        }
    }

    /// <summary>
    /// オプションUIを閉じる
    /// </summary>
    public void CloseOptionUI()
    {
        _isOptionUIActive = false;
    }

    /// <summary>
    /// VolumeUIを開く
    /// </summary>
    public void Volume()
    {
        IsVolumeUIActive = true;
    }

    /// <summary>
    /// VolumeUIを閉じる
    /// </summary>
    public void CloseVolumeUI()
    {
        IsVolumeUIActive = false;

        VolumeUI.SetActive(false);
    }

    /// <summary>
    /// KeyUIを開く
    /// </summary>
    public void Key()
    {
        IsKeyUIActive = true;
    }

    /// <summary>
    /// KeyUIを閉じる
    /// </summary>
    public void CloseKeyUI()
    {
        IsKeyUIActive = false;

        KeyUI.SetActive(false);
    }

    /// <summary>
    /// コンティニュー
    /// </summary>
    public void Continue()
    {
        _option.CloseOptionUI();
    }

    /// <summary>
    /// Backボタン
    /// </summary>
    public void Back()
    {
        if (IsVolumeUIActive)
        {
            CloseVolumeUI();
            return;
        }

        if (IsKeyUIActive)
        {
            CloseKeyUI();
            return;
        }
    }

    public void SetBGM(float volume)
    {
        _option.SetBGMVolume(volume);
        Debug.Log($"BGM Volume set to {volume}");
    }

    public void SetSE(float volume)
    {
        _option.SetSEVolume(volume);
        Debug.Log($"SE Volume set to {volume}");
    }

}
