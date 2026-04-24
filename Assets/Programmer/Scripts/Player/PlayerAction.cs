using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

public class PlayerAction : MonoBehaviour
{
    [SerializeField] private int initSoul = 5;//初期のソウルの数
    private int currentSoul;//現在のソウルの数

    [SerializeField] private  GameObject[] gimmickKind;//所持しているギミックの種類
    private List<KeyValuePair<GameObject, int>> gimmickList;//所持しているギミックの数

    private int currentGimmickIndex = 0;//現在選択しているギミック

    //プレイヤーのモード
    public enum PlayerMode
    {
        Normal,//通常
        Setting,//設置フェーズ
    };

    public PlayerMode currentMode { private set; get; }

    // Start is called before the first frame update
    void Start()
    {
        //現在のソウルの数
        currentSoul = initSoul;

        //現在のモード
        currentMode = PlayerMode.Normal;

        //ギミックごとに持っている数を初期化
        gimmickList = new List<KeyValuePair<GameObject, int>>();

        for (int i = 0 ; i < gimmickKind.Length ; ++i)
        {
            gimmickList.Add(new KeyValuePair<GameObject, int>(gimmickKind[i], 0));
        }
    }

    // Update is called once per frame
    void Update()
    {
        //キー操作でUIのギミックの選択
        if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentGimmickIndex++;

            if (currentGimmickIndex > gimmickList.Count)
                currentGimmickIndex = 0;
        }
        else if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentGimmickIndex--;

            if (currentGimmickIndex < 0)
                currentGimmickIndex = gimmickList.Count;
        }
        
        //モードの切り替え
        if(Input.GetKeyDown(KeyCode.Q))
        {
            switch (currentMode)
            {
                case PlayerMode.Normal:
                    currentMode = PlayerMode.Setting;
                    break;
                case PlayerMode.Setting:
                    SettingAction();
                    currentMode = PlayerMode.Normal;
                    break;
                default:
                    break;
            }
        }
    }

    private void SettingAction()
    {
        //ギミックの生成位置
        Vector3 settingPos = Vector3.zero;
        settingPos = transform.position + 
            new Vector3(transform.forward.x * transform.localScale.x,
                    0.0f,
                    transform.forward.z * transform.localScale.z);

        //グリッドの位置を取得
        var roomGrid = FindObjectOfType<RoomGrid>();

        Vector2Int grid = roomGrid.GetGridFromPos(settingPos);

        Vector3 gridPos = roomGrid.GetWorldPosFromGrid(grid);

        //生成
        Instantiate(gimmickList[currentGimmickIndex].Key, gridPos, Quaternion.identity);
  
    }

    //ソウルの数を加算する関数
    public void AddSoul(int addnum)
    {
        currentSoul += addnum;
    }
}
