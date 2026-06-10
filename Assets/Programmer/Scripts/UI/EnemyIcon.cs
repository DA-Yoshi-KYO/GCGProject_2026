using UnityEngine;
using UnityEngine.UI;

public class EnemyIcon : MonoBehaviour
{
    [Header("HPゲージのUI")]
    [SerializeField] private Image hp;

    private CS_ThiefAI thiefAI;

    void Update()
    {
        if (thiefAI == null)
        {
            return;
        }

            int current = thiefAI.read_Durability;
        int max = thiefAI.read_MaxDurability;

        if (max <= 0) return;

        hp.fillAmount = Mathf.Clamp01((float)current / max);
    }

    public void SetScript(CS_ThiefAI script)
    {
        thiefAI = script;

        if (hp != null)
        {
            hp.type = Image.Type.Filled;
            hp.fillMethod = Image.FillMethod.Horizontal;
            hp.fillOrigin = (int)Image.OriginHorizontal.Left;  // 左端を基点に右へ伸びる
            hp.fillAmount = 1f;
        }
    }

    public CS_ThiefAI GetScript() => thiefAI;
}
