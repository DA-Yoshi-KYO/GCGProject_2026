using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionSys : MonoBehaviour
{
    private const int BackToGameIndex = 2;
    private const int BackToTitleIndex = 3;

    [SerializeField] GameObject[] optionUI;
    [SerializeField] Sprite[] detailSprite;
    [SerializeField] GameObject detail;
    [SerializeField] GameObject bgmCursor;
    [SerializeField] GameObject seCursor;
    [SerializeField] Image bgmSoundBar;
    [SerializeField] Image seSoundBar;
    [SerializeField] Sprite closeImg;

    struct SelectGameObject
    {
        public GameObject TrueObject;
        public GameObject FalseObject;
    }

    List<SelectGameObject> selectGameObjects = new List<SelectGameObject>();
    [SerializeField] private int selected = 0;
    CustomInputAction inputAction;

    float seSoundValue = 0;
    float bgmSoundValue = 0;

    bool isInGame = false;

    private void Awake()
    {
        isInGame = SceneManager.GetActiveScene().name == "MainScene";

        if (!isInGame && optionUI.Length >= 4)
        {
            optionUI[BackToGameIndex].SetActive(false);
            Transform textTransform = optionUI[BackToTitleIndex].transform.Find("Text");
            if (textTransform != null)
            {
                Image textImage = textTransform.GetComponent<Image>();
                if (textImage != null)
                {
                    textImage.sprite = closeImg;
                }
            }
        }

        for (int i = 0; i < optionUI.Length; i++)
        {
            if (!isInGame && i == BackToGameIndex)
            {
                continue;
            }

            selectGameObjects.Add(new SelectGameObject
            {
                TrueObject = optionUI[i].transform.Find("Select_true").gameObject,
                FalseObject = optionUI[i].transform.Find("Select_false").gameObject
            });
        }

        inputAction = CS_CustomInputActionManager.instance.customInputAction;
        inputAction.Option.Up.started += Up;
        inputAction.Option.Down.started += Down;
        inputAction.Option.Decision.started += Enter;

        // OptionSysはOption.OpenOptionUI()自身がInstantiateして生成するため、
        // この時点でOption.Instanceは必ず存在している
        if (Option.Instance == null)
        {
            Debug.LogError("Optionが見つかりません。");
            return;
        }

        seSoundValue = Option.Instance.GetSEVolume();
        bgmSoundValue = Option.Instance.GetBGMVolume();

        UpdateSoundCursor(bgmSoundBar, bgmCursor);
        UpdateSoundCursor(seSoundBar, seCursor);
    }

    private void OnDestroy()
    {
        inputAction.Option.Up.started -= Up;
        inputAction.Option.Down.started -= Down;
        inputAction.Option.Decision.started -= Enter;
    }

    private void Update()
    {
        if (inputAction.Option.Left.IsPressed())
        {
            Left();
        }
        if (inputAction.Option.Right.IsPressed())
        {
            Right();
        }

        if (selected < 0)
        {
            selected = selectGameObjects.Count - 1;
        }
        else if (selected >= selectGameObjects.Count)
        {
            selected = 0;
        }

        bgmSoundBar.fillAmount = bgmSoundValue / 100f;
        seSoundBar.fillAmount = seSoundValue / 100f;

        if (selected >= 0 && selected < detailSprite.Length)
        {
            detail.GetComponent<Image>().sprite = detailSprite[selected];
        }

        UpdateSoundCursor(bgmSoundBar, bgmCursor);
        UpdateSoundCursor(seSoundBar, seCursor);

        for (int i = 0; i < selectGameObjects.Count; i++)
        {
            bool isSelected = i == selected;
            selectGameObjects[i].TrueObject.SetActive(isSelected);
            selectGameObjects[i].FalseObject.SetActive(!isSelected);
        }
    }

    /// <summary>
    /// 音量バーの見た目の値に合わせてカーソル位置を更新
    /// </summary>
    private void UpdateSoundCursor(Image soundBar, GameObject cursor)
    {
        Vector3 barPos = soundBar.transform.position;
        Vector2 barSize = soundBar.rectTransform.sizeDelta;

        float barLeftX = barPos.x - (barSize.x / 2.0f);
        float cursorX = barLeftX + (barSize.x * soundBar.fillAmount);

        Vector3 cursorPos = cursor.transform.position;
        cursorPos.x = cursorX;
        cursor.transform.position = cursorPos;
    }

    private void Up(InputAction.CallbackContext context)
    {
        selected--;
    }

    private void Down(InputAction.CallbackContext context)
    {
        selected++;
    }

    private void Enter(InputAction.CallbackContext context)
    {
        if (selected != BackToGameIndex && selected != BackToTitleIndex)
        {
            return;
        }

        if (Option.Instance == null)
        {
            Debug.LogError("Optionが見つかりません。");
            return;
        }

        Option.Instance.CloseOptionUI();

        if (selected == BackToTitleIndex)
        {
            CS_SceneTransition sceneTransition = FindFirstObjectByType<CS_SceneTransition>();
            if (sceneTransition != null)
            {
                sceneTransition.StartSceneTransition("TitleScene");
            }
            else
            {
                Debug.LogError("CS_SceneTransitionが見つかりません");
            }
        }
    }

    private void Left()
    {
        if (Option.Instance == null)
        {
            Debug.LogError("Optionが見つかりません。");
            return;
        }

        if (selected == 0)
        {
            bgmSoundValue = Mathf.Max(0f, Option.Instance.GetBGMVolume() - 0.1f);
            Option.Instance.SetBGMVolume(bgmSoundValue);
        }
        else if (selected == 1)
        {
            seSoundValue = Mathf.Max(0f, Option.Instance.GetSEVolume() - 0.1f);
            Option.Instance.SetSEVolume(seSoundValue);
        }
    }

    private void Right()
    {
        if (Option.Instance == null)
        {
            Debug.LogError("Optionが見つかりません。");
            return;
        }

        if (selected == 0)
        {
            bgmSoundValue = Mathf.Min(100f, Option.Instance.GetBGMVolume() + 0.1f);
            Option.Instance.SetBGMVolume(bgmSoundValue);
        }
        else if (selected == 1)
        {
            seSoundValue = Mathf.Min(100f, Option.Instance.GetSEVolume() + 0.1f);
            Option.Instance.SetSEVolume(seSoundValue);
        }
    }
}
