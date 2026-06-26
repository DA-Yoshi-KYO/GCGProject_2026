//|| GimmickManager.cs ||―――――――――――――――――――――――――――――――
//|| 作者 : 大瀧蓮
//||
//|| 更新 : 2026/05/24 作成開始
//|| 追加 : 2026/06/26 チュートリアル動画再生処理追加
//||
//|| ―――――――――――――――――――――――――――――――――――――――――
//||
//|| 概要 : ギミックのクールタイムとライフタイムを管理するクラス
//||        使用可能量を管理するクラス
//||        壊れた場合はクールタイムへ入り、クールタイム終了後に使用可能量が回復する
//||
//|| ―――――――――――――――――――――――――――――――――――――――――

using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.HID.HID;

public class GimmickManager : MonoBehaviour
{
    //=========================================================
    // ギミック種類ごとの設定情報
    //=========================================================
    [System.Serializable]
    public class GimmickInfo
    {
        // クールタイムの長さ
        public float coolTime;
        // 設置後の生存時間
        //public float lifeTime;
        // 同時設置可能数
        public int maxNum;
        // 現在設置可能な数
        public int currentNum;
        public GimmickInfo(float coolTime, int maxNum)
        {
            this.coolTime = coolTime;
            //this.lifeTime = lifeTime;
            this.maxNum = maxNum;

            // 初期状態では最大数まで置ける
            currentNum = maxNum;
        }
    }

    //=========================================================
    // 実際に設置されたギミック情報
    //=========================================================
    public class ActiveGimmick
    {
        public GimmickBase gimmick;
        public Gimmick gimmickType;

        public float coolTimer;
        public bool isCoolTime;
        public bool isEnd;
        public ActiveGimmick(
            GimmickBase gimmick)
        {
            this.gimmick = gimmick;
            gimmickType = gimmick.GetGimmickTag();

            coolTimer = 0.0f;

            isCoolTime = false;
            isEnd = false;
        }
    }

    //=========================================================
    // ギミック設定
    //=========================================================
    [System.Serializable]
    private class GimmickInfoData
    {
        public Gimmick gimmickTag;
        public GimmickInfo gimmickInfo;
        public GameObject itemPrefab;
    }

    [SerializeField]
    private List<GimmickInfoData> gimmickInfoList;

    private Dictionary<Gimmick, GimmickInfo> gimmickInfo =
        new Dictionary<Gimmick, GimmickInfo>();

    private Dictionary<Gimmick, bool> isItemGetNow = 
        new Dictionary<Gimmick, bool>();

    //=========================================================
    // 設置中ギミック
    //=========================================================
    private List<ActiveGimmick> activeGimmicks =
        new List<ActiveGimmick>();

    GimmickInfo info;

    //=========================================================
    // チュートリアル動画再生に必要な変数
    //=========================================================
    private GameObject cutSceneManager;

    private void Awake()
    {
        gimmickInfo = new Dictionary<Gimmick, GimmickInfo>();

        activeGimmicks.Clear();

        foreach (var data in gimmickInfoList)
        {
            gimmickInfo[data.gimmickTag] = data.gimmickInfo;
        }
    }

    private void Start()
    {

        cutSceneManager = GameObject.Find("CutSceneManager");
    }

    //=========================================================
    // 設置開始
    //=========================================================
    public bool SettingStart(GimmickBase gimmickBase)
    {
        Gimmick type = gimmickBase.GetGimmickTag();

        Debug.Log($"[SettingStart] Try Setting : {type}");

        // 設置可能か確認
        if (!IsSetting(type))
        {
            Debug.LogWarning(
                $"[Setting Failed] {type} : 設置可能数不足");
            return false;
        }

        info = gimmickInfo[type];

        // 設置可能数を減らす
        info.currentNum--;

        Debug.Log(
            $"[Set Success] {type}" +
            $" Remaining : {info.currentNum}/{info.maxNum}");

        // 実体追加
        ActiveGimmick active =
            new ActiveGimmick(gimmickBase);

        activeGimmicks.Add(active);
        Debug.Log(
            $"[Active Add] {type}");

        return true;
    }

    //=========================================================
    // Update
    //=========================================================
    void Update()
    {
        for (int i = activeGimmicks.Count - 1 ; i >= 0 ; i--)
        {
            ActiveGimmick active = activeGimmicks[i];

            // 既に終了済みなら飛ばす
            if (active.isEnd)
                continue;

            Gimmick type = active.gimmickType;
            info = gimmickInfo[type];

            // 稼働中 _________________________________________
            if (!active.isCoolTime)
            {
                if (active.gimmick != null &&
                    active.gimmick.gimmickState == GimmickState.Broken)
                {
                    // クールタイム開始
                    active.isCoolTime = true;
                    active.coolTimer = info.coolTime;
                }
            }
            // クールタイム中 _________________________________
            else
            {
                active.coolTimer -= Time.deltaTime;
                // クールタイム終了
                if (active.coolTimer <= 0.0f)
                {
                    // 設置可能数回復
                    info.currentNum++;
                    active.isEnd = true;
                    // リストから削除
                    activeGimmicks.RemoveAt(i);
                }
            }
        }
    }

    //=========================================================
    // OnTriggerEnter
    //=========================================================
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Item"))
            return;

        string itemName = other.gameObject.name.Replace("(Clone)", "");
        foreach (var data in gimmickInfoList)
        {
            if (data.itemPrefab.name == itemName)
            {
                string situation = "";

                // TODO: 映像の再生処理を追加
                // 大瀧くんはこの辺ちゃんとリファクタしてください
                switch (itemName)
                {
                    case "ItemEmptyChest":
                        situation = "ItemEmptyChest";
                        break;
                    case "ItemPitFall":
                        situation = "ItemPitFall";
                        break;
                    case "ItemHyperVoice":
                        situation = "ItemHyperVoice";
                        break;
                    default:
                        break;
                }

                cutSceneManager.GetComponent<CS_CutSceneVideo>().SetVideoInfo(situation);
                cutSceneManager.GetComponent<CS_CutSceneVideo>().PlayVideo();

                AddCurrentGimmick(data.gimmickTag);
                Destroy(other.gameObject);
                IsSetItemGetNow(data.gimmickTag, true);

                Debug.Log($"{data.gimmickTag}HaveAdd");
                break;
            }
        }
    }

    //=========================================================
    // 設置可能か
    //=========================================================
    public bool IsSetting(Gimmick gimmickTag)
    {
        // 登録されていない
        if (!gimmickInfo.ContainsKey(gimmickTag))
        {
            Debug.LogError(
                $"[IsSetting Error] {gimmickTag} : 未登録ギミック");
            Debug.Log(
                $"[Type FullName] {gimmickTag.GetType().FullName}");
            Debug.Log(
                $"[Dictionary Count] {gimmickInfo.Count}");
            foreach (var pair in gimmickInfo)
            {
                Debug.Log(
                    $"[Dictionary Key] {pair.Key}");
            }

            return false;
        }

        GimmickInfo data = gimmickInfo[gimmickTag];

        // 置ける数がない
        if (data.currentNum <= 0)
        {
            Debug.Log(
                $"[IsSetting] {gimmickTag} : 設置不可");

            return false;
        }

        Debug.Log(
            $"[IsSetting] {gimmickTag} : 設置可能");

        return true;
    }

    //=========================================================
    // ギミックのクラス単位での取得
    //=========================================================
    public List<ActiveGimmick> GetGimmickList()
    {
        return activeGimmicks;
    }

    //=========================================================
    // 最大所持数取得
    //=========================================================
    public int GetMaxNum(Gimmick gimmickTag)
    {
        if (!gimmickInfo.ContainsKey(gimmickTag))
        {
            Debug.LogError(
                $"[GetMaxNum Error] {gimmickTag} : 未登録");

            return 0;
        }
        return gimmickInfo[gimmickTag].maxNum;
    }
    //=========================================================
    // 最大設置数取得※現在の所持数。
    //=========================================================
    public int GetCurrentNum(Gimmick gimmickTag)
    {
        if (!gimmickInfo.ContainsKey(gimmickTag))
        {
            Debug.LogError(
                $"[GetCurrentNum Error] {gimmickTag} : 未登録");
            return 0;
        }
        return gimmickInfo[gimmickTag].currentNum;
    }

    //=========================================================
    // 特定のタグのギミックを取得した瞬間を判定
    //=========================================================
    private void IsSetItemGetNow(Gimmick gimmickTag, bool Getting)
    {
        isItemGetNow[gimmickTag] = Getting;
    }
    public bool IsGetItemGetNow(Gimmick gimmickTag)
    {
        bool isGetting = isItemGetNow[gimmickTag];
        IsSetItemGetNow(gimmickTag, false);
        return isGetting;
    }

    //=========================================================
    // クールタイム取得
    //==========================================================
    public float GetCoolTime(Gimmick gimmickTag)
    {//クールタイムの最大値を取得
        float maxTime = 0.0f;

        foreach (var active in activeGimmicks)
        {
            if (active.gimmickType == gimmickTag &&
                active.isCoolTime)
            {
                maxTime = Mathf.Max(maxTime, active.coolTimer);
            }
        }

        return maxTime;
    }

    //=========================================================
    // 所持数の変更
    //=========================================================
    public void SetMaxGimmick(Gimmick gimmickTag, int value)
    {//ギミックの最大値を設定
        gimmickInfo[gimmickTag].maxNum = value;
    }
    public void AddMaxGimmick(Gimmick gimmickTag)
    {//ギミックの最大値を追加
        gimmickInfo[gimmickTag].maxNum++;
    }
    public void SetCurrentGimmick(Gimmick gimmickTag, int value)
    {//ギミック所持数の設定
        gimmickInfo[gimmickTag].currentNum = value;
        //所持数が最大数を超えたら最大数を変更
        if(gimmickInfo[gimmickTag].maxNum < gimmickInfo[gimmickTag].currentNum)
        {
            gimmickInfo[gimmickTag].maxNum = gimmickInfo[gimmickTag].currentNum;
        }
    }
    public void AddCurrentGimmick(Gimmick gimmickTag)
    {//ギミック所持数の追加
        gimmickInfo[gimmickTag].currentNum++;
        //所持数が最大数を超えたら最大数を変更
        if (gimmickInfo[gimmickTag].maxNum < gimmickInfo[gimmickTag].currentNum)
        {
            gimmickInfo[gimmickTag].maxNum = gimmickInfo[gimmickTag].currentNum;
        }
    }

    //=========================================================
    // 全削除
    //=========================================================
    public void ClearAll()
    {
        Debug.Log("=== ClearAll Start ===");

        for (int i = 0 ; i < activeGimmicks.Count ; i++)
        {
            if (activeGimmicks[i].gimmick != null)
            {
                Debug.Log(
                    $"[Force Destroy] " +
                    $"{activeGimmicks[i].gimmick.GetGimmickTag()}");

                Destroy(activeGimmicks[i].gimmick.gameObject);
            }
        }

        activeGimmicks.Clear();

        foreach (var pair in gimmickInfo)
        {
            pair.Value.currentNum = pair.Value.maxNum;

            Debug.Log(
                $"[Reset Count] {pair.Key}" +
                $" : {pair.Value.currentNum}");
        }

        Debug.Log("=== ClearAll End ===");
    }
    public void SetGimmickState(GimmickBase gimmick, GimmickState state)
    {
        foreach (var active in activeGimmicks)
        {
            if (active.gimmick == gimmick)
            {
                active.gimmick.gimmickState = state;
            }
        }
    }
}
