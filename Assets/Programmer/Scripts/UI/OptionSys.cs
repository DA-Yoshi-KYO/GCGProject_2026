// using UnityEditor.Experimental.GraphView; // ← 削除（Editor専用。ビルドエラーの原因）
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class OptionSys : MonoBehaviour
{
    [SerializeField] private string ManagerName = "GameOptionManager";

    [SerializeField] private GameObject UI;
    [SerializeField] private GameObject VolumeUI;
    [SerializeField] private GameObject KeyUI;
    [SerializeField] private GameObject SelectedUI;
    [SerializeField] private GameObject[] OptionButtons;
    [SerializeField] private GameObject[] VolumeObject;
    [SerializeField] private GameObject[] KeyObject;
    [SerializeField] private Slider BGM;
    [SerializeField] private Slider SE;

    [SerializeField] private float animationSpeed = 5.0f;
    // 修正5: 音量変化速度を定数化（フレームレート依存を防ぐ）
    [SerializeField] private float volumeChangeSpeed = 0.5f;

    private GameObject _optionManager;
    private Option _option;

    private bool _isOptionUIActive = true;
    private int _selectedIndex = 0;
    private int _volumeSelectedIndex = 0;
    private int _keySelectedIndex = 0;
    private CustomInputAction inputActions;

    public bool IsVolumeUIActive { get; private set; } = false;
    public bool IsKeyUIActive { get; private set; } = false;

    private void Awake()
    {
        inputActions = new CustomInputAction();
        inputActions.Enable();
        inputActions.Option.Up.started += Up;
        inputActions.Option.Down.started += Down;
        inputActions.Option.Decision.started += Enter;

        _optionManager = GameObject.Find(ManagerName);

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

        VolumeUI.SetActive(false);
        KeyUI.SetActive(false);

        IsVolumeUIActive = false;
        IsKeyUIActive = false;

        UI.transform.localScale = Vector3.zero;
    }

    // 修正4: OnDestroyでイベント解除・InputActions破棄
    private void OnDestroy()
    {
        if (inputActions != null)
        {
            inputActions.Option.Up.performed -= Up;
            inputActions.Option.Down.performed -= Down;
            inputActions.Option.Decision.performed -= Enter;
            inputActions.Disable();
            inputActions.Dispose();
        }
    }

    private void Update()
    {
        float delta = Time.unscaledDeltaTime;

        if (_isOptionUIActive)
        {
            UI.transform.localScale = Vector3.Lerp(
                UI.transform.localScale,
                Vector3.one,
                animationSpeed * delta
            );

            if (Vector3.Distance(UI.transform.localScale, Vector3.one) < 0.01f)
            {
                UI.transform.localScale = Vector3.one;
            }

            if (UI.transform.localScale.x < 0.95f)
            {
                return;
            }

            bool isSubUIActive = false;

            if (IsVolumeUIActive)
            {
                VolumeUI.SetActive(true);
                isSubUIActive = true;
            }
            else
            {
                VolumeUI.SetActive(false);
            }

            if (IsKeyUIActive)
            {
                KeyUI.SetActive(true);
                isSubUIActive = true;
            }
            else
            {
                KeyUI.SetActive(false);
            }

            UI.SetActive(!isSubUIActive);
        }
        else
        {
            UI.transform.localScale = Vector3.Lerp(
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

        if (IsVolumeUIActive)
        {
            SelectedUI.transform.position = VolumeObject[_volumeSelectedIndex].transform.position;

            if (_volumeSelectedIndex == 0)
            {
                if (inputActions.Option.Left.IsPressed())
                {
                    // 修正1: delta を掛けてフレームレート非依存に。Mathf.Clampで範囲保証
                    BGM.value = Mathf.Clamp(BGM.value - volumeChangeSpeed * delta, BGM.minValue, BGM.maxValue);
                    SetBGM(BGM.value);
                }
                if (inputActions.Option.Right.IsPressed())
                {
                    BGM.value = Mathf.Clamp(BGM.value + volumeChangeSpeed * delta, BGM.minValue, BGM.maxValue);
                    SetBGM(BGM.value);
                }
            }
            if (_volumeSelectedIndex == 1)
            {
                if (inputActions.Option.Left.IsPressed())
                {
                    SE.value = Mathf.Clamp(SE.value - volumeChangeSpeed * delta, SE.minValue, SE.maxValue);
                    SetSE(SE.value);
                }
                if (inputActions.Option.Right.IsPressed())
                {
                    SE.value = Mathf.Clamp(SE.value + volumeChangeSpeed * delta, SE.minValue, SE.maxValue);
                    SetSE(SE.value);
                }
            }
            return;
        }

        if (IsKeyUIActive)
        {
            SelectedUI.transform.position = KeyObject[_keySelectedIndex].transform.position;
            return;
        }

        if (OptionButtons.Length > _selectedIndex)
        {
            SelectedUI.transform.position = OptionButtons[_selectedIndex].transform.position;
        }
    }

    public void CloseOptionUI()
    {
        _isOptionUIActive = false;
    }

    public void Volume()
    {
        IsVolumeUIActive = true;
        _volumeSelectedIndex = 0;
    }

    public void CloseVolumeUI()
    {
        IsVolumeUIActive = false;
        VolumeUI.SetActive(false);
    }

    public void Key()
    {
        IsKeyUIActive = true;
        _keySelectedIndex = 0;
    }

    public void CloseKeyUI()
    {
        IsKeyUIActive = false;
        KeyUI.SetActive(false);
    }

    public void Continue()
    {
        _option.CloseOptionUI();
    }

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

        CloseOptionUI();
        // 修正3: FindObjectOfType → FindFirstObjectByType（Unity 2023以降の推奨API）
        CS_SceneTransition sceneTransition = GameObject.FindFirstObjectByType<CS_SceneTransition>();
        if (sceneTransition != null)
        {
            sceneTransition.StartSceneTransition("TitleScene");
        }
        else
        {
            Debug.LogError("CS_SceneTransitionが見つかりません");
        }
    }

    private void Up(InputAction.CallbackContext context)
    {
        if (IsVolumeUIActive)
        {
            _volumeSelectedIndex--;
            if (_volumeSelectedIndex < 0) _volumeSelectedIndex = VolumeObject.Length - 1;
            return;
        }
        if (IsKeyUIActive)
        {
            _keySelectedIndex--;
            if (_keySelectedIndex < 0) _keySelectedIndex = KeyObject.Length - 1;
            return;
        }
        _selectedIndex--;
        if (_selectedIndex < 0) _selectedIndex = OptionButtons.Length - 1;
    }

    private void Down(InputAction.CallbackContext context)
    {
        if (IsVolumeUIActive)
        {
            _volumeSelectedIndex++;
            if (_volumeSelectedIndex >= VolumeObject.Length) _volumeSelectedIndex = 0;
            return;
        }
        if (IsKeyUIActive)
        {
            _keySelectedIndex++;
            if (_keySelectedIndex >= KeyObject.Length) _keySelectedIndex = 0;
            return;
        }
        _selectedIndex++;
        if (_selectedIndex >= OptionButtons.Length) _selectedIndex = 0;
    }

    private void Enter(InputAction.CallbackContext context)
    {
        if (IsVolumeUIActive)
        {
            if (_volumeSelectedIndex == 2) CloseVolumeUI();
            return;
        }
        if (IsKeyUIActive)
        {
            if (_keySelectedIndex == 1) CloseKeyUI();
            return;
        }
        switch (_selectedIndex)
        {
            case 0: Continue(); break;
            case 1: Volume(); break;
            case 2: Key(); break;
            case 3: Back(); break;
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
