using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
// 制作者　秋野

//　OptionUIの状態の取得関数追加　元浪

// シングルトンクラス
// サウンドやゲーム設定に関するデータを管理するクラス

public class Option : MonoBehaviour
{
    private static Option _instance;

    [SerializeField] private float _bgmVolume = 100.0f; // BGMの音量
    [SerializeField] private float _seVolume = 100.0f;  // SEの音量
    [SerializeField] private GameObject OptionUI;
    private GameObject GameUI;

    private bool _isOptionUIActive = false; // OptionUIがアクティブかどうか

    private CustomInputAction _input;

    private GameObject _prevOption;

    /// <summary>
    /// インスタンスを取得するプロパティ
    /// </summary>
    public static Option Instance
    {
        get
        {
            return _instance;
        }
    }

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
        _input = new CustomInputAction();

        // Option入力のトリガー登録
        _input.Player.Option.performed += OnOption;

        // Input有効化
        _input.Enable();

        GameObject canvase = GameObject.Find("Canvases");
        GameUI = canvase.transform.Find("GameUICavas").gameObject;

    }

    /// <summary>
    /// オプションボタンが押された時
    /// </summary>
    private void OnOption(InputAction.CallbackContext ctx)
    {
        // タイトルシーンではオプションUIを開かない
        if (SceneManager.GetActiveScene().name.ToString() == "Title") return;
        if (!_isOptionUIActive)
        {
            OpenOptionUI();
        }
        else
        {
            if(_prevOption== null)
            {
                Debug.LogError("OptionUIが存在しません。");
                return;
            }
            OptionSys optionSys = _prevOption.GetComponent<OptionSys>();
            if (optionSys == null)
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
        _isOptionUIActive = true;

        if (_prevOption == null)
        {
            // Canvasを探してその子オブジェクトとしてOptionUIを生成
            GameObject canvas = GameObject.Find("OptionCanvas");
            _prevOption = Instantiate(OptionUI, Vector3.zero, Quaternion.identity, canvas.transform);
            RectTransform rectTransform = _prevOption.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = Vector2.zero;
        }

        if(GameUI == null)
        {
            GameObject canvase = GameObject.Find("Canvases");
            GameUI = canvase.transform.Find("GameUICavas").gameObject;
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
        if (GameUI == null)
        {
            GameObject canvase = GameObject.Find("Canvases");
            GameUI = canvase.transform.Find("GameUICavas").gameObject;
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

    private void OnDestroy()
    {
        if (_input != null)
        {
            _input.Player.Option.performed -= OnOption;

            _input.Disable();
        }
    }
}
