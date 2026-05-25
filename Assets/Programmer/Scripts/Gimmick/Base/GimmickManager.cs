

using UnityEngine;
using System.Collections.Generic;

public class GimmickManager : MonoBehaviour
{
    private GimmickBase gimmick;

    public class GimmickInfo
    {
        public float coolTime;
        public float lifeTime;
        public int maxNum;

        public GimmickInfo(float coolTime, float lifeTime, int maxNum)
        {
            this.coolTime = coolTime;
            this.lifeTime  = lifeTime;
            this.maxNum = maxNum;
        }
    }
    Dictionary<Gimmick, GimmickInfo> gimmickInfo = new Dictionary<Gimmick, GimmickInfo>();

    float currentCoolTime;
    float currentLifeTime;

    // Start is called before the first frame update
    void Start()
    {
        gimmick = GetComponent<GimmickBase>();
        gimmickInfo.Clear();
        //ギミックステートの追加
        gimmickInfo.Add(Gimmick.Pot, new GimmickInfo(5f, 10f, 5));
        gimmickInfo.Add(Gimmick.IronBall, new GimmickInfo(10f, 15f, 2));
        gimmickInfo.Add(Gimmick.EmptyChest, new GimmickInfo(10f, 20f, 2));

        currentCoolTime = 0; currentLifeTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        //if(gimmick.gi)
        //if (gimmick.gimmickState == GimmickState.Active)
        //{
        //    currentLifeTime += Time.deltaTime;
        //}
    }

    //設置可能かどうかを取得する
    public bool IsSetting(Gimmick gT)
    {
        GimmickInfo data = gimmickInfo[gT];

        if (data.coolTime <= 0 || 0 >= data.maxNum)
        {
            return false;
        }
        return true;
    }
}
