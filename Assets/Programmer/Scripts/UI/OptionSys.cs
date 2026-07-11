using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static HoudiniEngineUnity.HEU_InputNode;

public class OptionSys : MonoBehaviour
{
    [SerializeField] string optionManagerStr = "";
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
        public GameObject Object_True;
        public GameObject Object_False;
    }

    List<SelectGameObject> selectGameObjects = new List<SelectGameObject>();
    [SerializeField] private int selected = 0;
    CustomInputAction inputAction;
    GameObject optionMangerObj;
    Option option;

    float seSoundValue = 0;
    float bgmSoundValue = 0;

    bool isInGame = false;

    private void Awake()
    {
        string sceneName = SceneManager.GetActiveScene().name.ToString();
        if (sceneName == "MainScene")
        {
            isInGame = true;
        }

        if (!isInGame && optionUI.Length >= 4)
        {
            optionUI[2].SetActive(false);
            Transform textTransform = optionUI[3].transform.Find("Text");
            if (textTransform != null)
            {
                Image textImge = textTransform.GetComponent<Image>();
                if (textImge != null)
                {
                    textImge.sprite = closeImg;
                }
            }
        }

        for (int i = 0 ; i < optionUI.Length ; i++)
        {
            if(!isInGame && i == 2)
            {
                continue;
            }
            SelectGameObject TempSelectObject = new SelectGameObject();
            TempSelectObject.Object_True = optionUI[i].transform.Find("Select_true").gameObject;
            TempSelectObject.Object_False = optionUI[i].transform.Find("Select_false").gameObject;
            selectGameObjects.Add(TempSelectObject);
        }
        inputAction = new CustomInputAction();
        inputAction.Enable();
        inputAction.Option.Up.started += Up;
        inputAction.Option.Down.started += Down;
        inputAction.Option.Decision.started += Enter;

        optionMangerObj = GameObject.Find(optionManagerStr);
        if (optionMangerObj == null)
        {
            Debug.Log("OptionManagerが見つかりません");
            return;
        }
        option = optionMangerObj.GetComponent<Option>();
        if (option == null)
        {
            Debug.Log("Optionコンポーネントが見つかりません");
            return;
        }

        seSoundValue = option.GetSEVolume();
        bgmSoundValue = option.GetBGMVolume();

        Vector3 bgmSoundBarPos = bgmSoundBar.transform.position;
        Vector2 bgmSoundBarSize = bgmSoundBar.rectTransform.sizeDelta;

        float barLeftX = bgmSoundBarPos.x - (bgmSoundBarSize.x / 2.0f);
        float tempX = barLeftX + (bgmSoundBarSize.x * bgmSoundBar.fillAmount);

        Vector3 cursor = bgmCursor.transform.position;
        cursor.x = tempX;
        bgmCursor.transform.position = cursor;



        Vector3 seSoundBarPos = seSoundBar.transform.position;
        Vector2 seSoundBarSize = seSoundBar.rectTransform.sizeDelta;

        float barLeftX2 = seSoundBarPos.x - (seSoundBarSize.x / 2.0f);
        float tempX2 = barLeftX2 + (seSoundBarSize.x * seSoundBar.fillAmount);

        Vector3 cursor2 = seCursor.transform.position;
        cursor2.x = tempX2;
        seCursor.transform.position = cursor2;
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
        else if(selected >= selectGameObjects.Count)
        {
            selected = 0;
        }

        bgmSoundBar.fillAmount = bgmSoundValue / 100f;
        seSoundBar.fillAmount = seSoundValue / 100f;

        Image image = detail.GetComponent<Image>();
        if (selected >= 0 && selected < detailSprite.Length)
        {
            image.sprite = detailSprite[selected];
        }

        // Cursor計算
        Vector3 bgmSoundBarPos = bgmSoundBar.transform.position;
        Vector2 bgmSoundBarSize = bgmSoundBar.rectTransform.sizeDelta;

        float barLeftX = bgmSoundBarPos.x - (bgmSoundBarSize.x / 2.0f);
        float tempX = barLeftX + (bgmSoundBarSize.x * bgmSoundBar.fillAmount);

        Vector3 cursor = bgmCursor.transform.position;
        cursor.x = tempX;
        bgmCursor.transform.position = cursor;

        Vector3 seSoundBarPos = seSoundBar.transform.position;
        Vector2 seSoundBarSize = seSoundBar.rectTransform.sizeDelta;

        float barLeftX2 = seSoundBarPos.x - (seSoundBarSize.x / 2.0f);
        float tempX2 = barLeftX2 + (seSoundBarSize.x * seSoundBar.fillAmount);

        Vector3 cursor2 = seCursor.transform.position;
        cursor2.x = tempX2;
        seCursor.transform.position = cursor2;

        for (int i = 0 ; i < selectGameObjects.Count ; i++)
        {
            if (i == selected)
            {
                selectGameObjects[i].Object_True.SetActive(true);
                selectGameObjects[i].Object_False.SetActive(false);
            }
            else
            {
                selectGameObjects[i].Object_True.SetActive(false);
                selectGameObjects[i].Object_False.SetActive(true);
            }
        }
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
        string DebugStr = "Enterが押されました。選択中の項目は" + selected + "です。";
        Debug.Log(DebugStr);
        switch (selected)
        {
            case 0:
                break;
            case 1:
                break;
            case 2:
            {
                if (option == null)
                {
                    if (optionMangerObj == null)
                    {
                        optionMangerObj = GameObject.Find(optionManagerStr);
                        if (optionMangerObj == null)
                        {
                            Debug.Log("OptionManagerが見つかりません");
                            return;
                        }
                    }
                    option = optionMangerObj.GetComponent<Option>();
                    if (option == null)
                    {
                        Debug.Log("Optionコンポーネントが見つかりません");
                        return;
                    }
                }
                option.CloseOptionUI();
            }
                break;
            case 3:
                {
                    if (option == null)
                    {
                        if (optionMangerObj == null)
                        {
                            optionMangerObj = GameObject.Find(optionManagerStr);
                            if (optionMangerObj == null)
                            {
                                Debug.Log("OptionManagerが見つかりません");
                                return;
                            }
                        }
                        option = optionMangerObj.GetComponent<Option>();
                        if (option == null)
                        {
                            Debug.Log("Optionコンポーネントが見つかりません");
                            return;
                        }
                    }
                    option.CloseOptionUI();
                    CS_SceneTransition sceneTransition = FindFirstObjectByType<CS_SceneTransition>();
                    if (sceneTransition != null)
                    {
                        sceneTransition.StartSceneTransition("TitleScene");
                    }
                    else
                    {
                        Debug.LogError("CS_SceneTransitionが見つかりません");
                    }
                    break;
                }
        }
    }

    private void Left()
    {
        if (option == null)
        {
            if (optionMangerObj == null)
            {
                optionMangerObj = GameObject.Find(optionManagerStr);
                if (optionMangerObj == null)
                {
                    Debug.Log("OptionManagerが見つかりません");
                    return;
                }
            }
            option = optionMangerObj.GetComponent<Option>();
            if (option == null)
            {
                Debug.Log("Optionコンポーネントが見つかりません");
                return;
            }
        }


        if (selected == 0)
        {
            bgmSoundValue = option.GetBGMVolume();
            bgmSoundValue -= 0.1f;
            if (bgmSoundValue < 0) bgmSoundValue = 0;
            option.SetBGMVolume(bgmSoundValue);
        }
        else if (selected == 1)
        {
            seSoundValue = option.GetSEVolume();
            seSoundValue -= 0.1f;
            if (seSoundValue < 0) seSoundValue = 0;
            option.SetSEVolume(seSoundValue);
        }
    }

    private void Right()
    {
        if (option == null)
        {
            if (optionMangerObj == null)
            {
                optionMangerObj = GameObject.Find(optionManagerStr);
                if (optionMangerObj == null)
                {
                    Debug.Log("OptionManagerが見つかりません");
                    return;
                }
            }
            option = optionMangerObj.GetComponent<Option>();
            if (option == null)
            {
                Debug.Log("Optionコンポーネントが見つかりません");
                return;
            }
        }

        if (selected == 0)
        {
            bgmSoundValue = option.GetBGMVolume();
            bgmSoundValue += 0.1f;
            if (bgmSoundValue > 100) bgmSoundValue = 100;
            option.SetBGMVolume(bgmSoundValue);
        }
        else if (selected == 1)
        {
            seSoundValue = option.GetSEVolume();
            seSoundValue += 0.1f;
            if (seSoundValue > 100) seSoundValue = 100;
            option.SetSEVolume(seSoundValue);
        }
    }

    private void OnDestroy()
    {
        if (inputAction != null)
        {
            inputAction.Option.Up.started -= Up;
            inputAction.Option.Down.started -= Down;
            inputAction.Option.Decision.started -= Enter;
            inputAction.Disable();
            inputAction.Dispose();
        }
    }
}
