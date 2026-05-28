//|| GimmickBase.cs ||――――――――――――
//|| 作者 : 大瀧蓮
//||
//|| 更新 : 2026/05/24 作成開始
//||―――――――――――――――――――――
//|| 概要 : ギミックのクールタイムとライフタイムを管理するクラス
//||        使用可能量を管理するクラス
//||        壊れた場合はクールタイムへ入り、クールタイム終了後に使用可能量が回復する
//||―――――――――――――――――――――

using UnityEngine;
using System.Collections.Generic;

public class GimmickManager : MonoBehaviour
{
    //=========================================================
    // ギミック種類ごとの設定情報
    //=========================================================

    public class GimmickInfo
    {
        // クールタイムの長さ
        public float coolTime;

        // 設置後の生存時間
        public float lifeTime;

        // 同時設置可能数
        public int maxNum;

        // 現在設置可能な数
        public int currentNum;

        public GimmickInfo(float coolTime, float lifeTime, int maxNum)
        {
            this.coolTime = coolTime;
            this.lifeTime = lifeTime;
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

        // ←追加
        public Gimmick gimmickType;

        public float lifeTimer;
        public float coolTimer;

        public bool isCoolTime;
        public bool isEnd;

        public ActiveGimmick(
            GimmickBase gimmick,
            float lifeTime)
        {
            this.gimmick = gimmick;

            // ←生成時に保存
            gimmickType = gimmick.GetGimmickTag();

            lifeTimer = lifeTime;
            coolTimer = 0.0f;

            isCoolTime = false;
            isEnd = false;
        }
    }

    //=========================================================
    // ギミック設定
    //=========================================================
    private Dictionary<Gimmick, GimmickInfo> gimmickInfo =
        new Dictionary<Gimmick, GimmickInfo>();

    //=========================================================
    // 設置中ギミック
    //=========================================================
    private List<ActiveGimmick> activeGimmicks =
        new List<ActiveGimmick>();

    GimmickInfo info;

    //=========================================================
    // Start
    //=========================================================
    void Start()
    {
        gimmickInfo.Clear();
        activeGimmicks.Clear();

        Debug.Log("=== GimmickManager Initialize Start ===");

        //=====================================================
        // ギミック登録
        //=====================================================
        gimmickInfo.Add(
            Gimmick.Pot,
            new GimmickInfo(5f, 10f, 5));

        Debug.Log(
            "[Register] Pot" +
            " CoolTime : 5" +
            " LifeTime : 10" +
            " MaxNum : 5");

        gimmickInfo.Add(
            Gimmick.IronBall,
            new GimmickInfo(10f, 15f, 2));

        Debug.Log(
            "[Register] IronBall" +
            " CoolTime : 10" +
            " LifeTime : 15" +
            " MaxNum : 2");

        gimmickInfo.Add(
            Gimmick.EmptyChest,
            new GimmickInfo(10f, 20f, 2));

        Debug.Log(
            "[Register] EmptyChest" +
            " CoolTime : 10" +
            " LifeTime : 20" +
            " MaxNum : 2");

        Debug.Log("=== GimmickManager Initialize End ===");
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
            new ActiveGimmick(gimmickBase, info.lifeTime);

        activeGimmicks.Add(active);

        Debug.Log(
            $"[Active Add] {type}" +
            $" LifeTime : {info.lifeTime}");

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
            {
                continue;
            }

            Gimmick type = active.gimmickType;
            info = gimmickInfo[type];
            //Debug.Log(
            //            $"[Recover Count] {type}" +
            //            $" Remaining : " +
            //            $"{info.currentNum}/{info.maxNum}");

            //-------------------------------------------------
            // 稼働中
            //-------------------------------------------------
            if (!active.isCoolTime)
            {
                // ライフタイム減少
                active.lifeTimer -= Time.deltaTime;

                if (active.gimmick != null &&
                    active.gimmick.gimmickState == GimmickState.Broken)
                {
                    // クールタイム開始
                    active.isCoolTime = true;
                    active.coolTimer = info.coolTime;
                }
                if (active.lifeTimer <= 0.0f)
                {
                    // クールタイム開始
                    active.isCoolTime = true;
                    active.coolTimer = info.coolTime;

                    active.gimmick.gimmickState = GimmickState.Broken;

                    Debug.Log("ライフタイム終了" + active.gimmickType);
                }
            }
            //-------------------------------------------------
            // クールタイム中
            //-------------------------------------------------
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

                    //Debug.Log(
                    //    $"[CoolTime End] {type}" +
                    //    $" Remaining : " +
                    //    $"{info.currentNum}/{info.maxNum}");
                    //Debug.Log(
                    //    $"[Remove ActiveGimmick] {type}");
                }
            }
        }
    }

    //=========================================================
    // 設置可能か
    //=========================================================
    public bool IsSetting(Gimmick gT)
    {
        // 登録されていない
        if (!gimmickInfo.ContainsKey(gT))
        {
            Debug.LogError(
                $"[IsSetting Error] {gT} : 未登録ギミック");
            Debug.Log(
                $"[Type FullName] {gT.GetType().FullName}");
            Debug.Log(
                $"[Dictionary Count] {gimmickInfo.Count}");
            foreach (var pair in gimmickInfo)
            {
                Debug.Log(
                    $"[Dictionary Key] {pair.Key}");
            }

            return false;
        }

        GimmickInfo data = gimmickInfo[gT];

        // 置ける数がない
        if (data.currentNum <= 0)
        {
            Debug.Log(
                $"[IsSetting] {gT} : 設置不可");

            return false;
        }

        Debug.Log(
            $"[IsSetting] {gT} : 設置可能");

        return true;
    }

    //=========================================================
    // 残り設置可能数取得
    //=========================================================
    public int GetRemainNum(Gimmick gT)
    {
        if (!gimmickInfo.ContainsKey(gT))
        {
            Debug.LogError(
                $"[GetRemainNum Error] {gT} : 未登録");

            return 0;
        }

        Debug.Log(
            $"[GetRemainNum] {gT}" +
            $" : {gimmickInfo[gT].currentNum}");

        return gimmickInfo[gT].currentNum;
    }

    //=========================================================
    // 最大設置数取得
    //=========================================================
    public int GetMaxNum(Gimmick gT)
    {
        if (!gimmickInfo.ContainsKey(gT))
        {
            Debug.LogError(
                $"[GetMaxNum Error] {gT} : 未登録");

            return 0;
        }

        Debug.Log(
            $"[GetMaxNum] {gT}" +
            $" : {gimmickInfo[gT].maxNum}");

        return gimmickInfo[gT].maxNum;
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
