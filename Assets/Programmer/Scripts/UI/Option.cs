using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
// 制作者　秋野

//　OptionUIの状態の取得関数追加　元浪

// シングルトンクラス
// サウンドやゲーム設定に関するデータを管理するクラス

public class Option : MonoBehaviour
{
    private const string CanvasesObjectName = "Canvases";
    private const string GameUICanvasName = "GameUICavas";
    private const string OptionCanvasObjectName = "OptionCanvas";

    private static Option _instance;

    [SerializeField] private float _bgmVolume = 100.0f; // BGMの音量
    [SerializeField] private float _seVolume = 100.0f;  // SEの音量
    [SerializeField] private GameObject OptionUI;
    [SerializeField] private Volume volume;
    DepthOfField dof;
    private GameObject GameUI;

    private bool _isOptionUIActive = false; // OptionUIがアクティブかどうか

    private CustomInputAction _input;

    private GameObject _prevOption;

    /// <summary>
    /// インスタンスを取得するプロパティ
    /// </summary>
    public static Option Instance => _instance;

    /// <summary>
    /// インスタンスを破棄
    /// </summary>
    public static void DestroyInstance()
    {
        if (_instance != null)
        {
            Destroy(_instance.gameObject);
            _instance = null;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;

            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }

        // Input生成
        _input = CS_CustomInputActionManager.instance.customInputAction;

        // Option入力のトリガー登録
        _input.Player.Option.performed += OnOption;

        GameUI = FindGameUI();

        if (volume == null)
        {
            Debug.LogError("volumeがアタッチされていません");
            return;
        }
        if (!volume.profile.TryGet<DepthOfField>(out dof))
        {
            Debug.LogError("volumeにdofがありません");
            return;
        }
        dof.active = false;
    }

    private void OnDestroy()
    {
        if (_input != null)
        {
            _input.Player.Option.performed -= OnOption;
        }
    }

    /// <summary>
    /// "Canvases/GameUICavas" を探して取得する
    /// </summary>
    private GameObject FindGameUI()
    {
        GameObject canvases = GameObject.Find(CanvasesObjectName);
        if (canvases == null)
        {
            Debug.Log($"{CanvasesObjectName}オブジェクトが見つかりません。");
            return null;
        }

        Transform gameUITransform = canvases.transform.Find(GameUICanvasName);
        if (gameUITransform == null)
        {
            Debug.Log($"{GameUICanvasName}オブジェクトが見つかりません。");
            return null;
        }

        return gameUITransform.gameObject;
    }

    /// <summary>
    /// オプションボタンが押された時
    /// </summary>
    private void OnOption(InputAction.CallbackContext ctx)
    {
        // タイトルシーンではオプションUIを開かない
        if (SceneManager.GetActiveScene().name == "TitleScene") return;

        if (!_isOptionUIActive)
        {
            OpenOptionUI();
        }
        else
        {

            if (_prevOption == null)
            {
                Debug.LogError("OptionUIが存在しません。");
                return;
            }
            if (_prevOption.GetComponent<OptionSys>() == null)
            {
                Debug.LogError("OptionUIにOptionSysコンポーネントがアタッチされていません。");
                return;
            }

            CloseOptionUI();
        }
    }

    /// <summary>
    /// オプションUIを開く
    /// </summary>
    public void OpenOptionUI()
    {
        dof.active = true;
        _isOptionUIActive = true;

        if (_prevOption == null)
        {
            // Canvasを探してその子オブジェクトとしてOptionUIを生成
            GameObject canvas = GameObject.Find(OptionCanvasObjectName);
            if (canvas == null)
            {
                Debug.LogError($"{OptionCanvasObjectName}オブジェクトが見つかりません。");
                return;
            }
            _prevOption = Instantiate(OptionUI, Vector3.zero, Quaternion.identity, canvas.transform);
            RectTransform rectTransform = _prevOption.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = Vector2.zero;
        }

        if (GameUI == null)
        {
            GameUI = FindGameUI();
        }
        if (GameUI != null)
        {
            GameUI.SetActive(false);
        }

        // ゲーム停止
        Time.timeScale = 0f;
    }

    /// <summary>
    /// オプションUIを閉じる
    /// </summary>
    public void CloseOptionUI()
    {
        dof.active = false;

        if (GameUI == null)
        {
            GameUI = FindGameUI();
        }
        if (GameUI != null)
        {
            GameUI.SetActive(true);
        }

        Destroy(_prevOption);

        _prevOption = null;

        _isOptionUIActive = false;
        Time.timeScale = 1f;
    }

    /// <summary>
    /// BGMの音量を設定
    /// </summary>
    /// <param name="volume">0～100</param>
    public void SetBGMVolume(float volume)
    {
        if (volume < 0.0f || volume > 100.0f)
        {
            Debug.LogError("BGMの音量は0から100の範囲で設定してください。");
            return;
        }

        _bgmVolume = volume;
    }

    /// <summary>
    /// SEの音量を設定
    /// </summary>
    /// <param name="volume">0～100</param>
    public void SetSEVolume(float volume)
    {
        if (volume < 0.0f || volume > 100.0f)
        {
            Debug.LogError("SEの音量は0から100の範囲で設定してください。");
            return;
        }

        _seVolume = volume;
    }

    /// <summary>
    /// BGM音量取得
    /// </summary>
    public float GetBGMVolume()
    {
        return _bgmVolume;
    }

    /// <summary>
    /// SE音量取得
    /// </summary>
    public float GetSEVolume()
    {
        return _seVolume;
    }

    /// <summary>
    /// OptionUIの状態設定
    /// </summary>
    public void SetIsOptionUIActive(bool isActive)
    {
        _isOptionUIActive = isActive;
    }

    /// <summary>
    /// OptionUIの状態の取得
    /// </summary>
    public bool GetIsOptionUIActive()
    {
        return _isOptionUIActive;
    }
}
