using UnityEngine;

public static class CS_EffectPlayUtility
{
    public static CSAD_EffectCommonProcessBase PlaySingleAndDestroy(
        GameObject effectPrefab,
        Vector3 position,
        Quaternion rotation,
        Transform parent,
        ref CSAD_EffectCommonProcessBase currentEffect)
    {
        if (effectPrefab == null)
        {
            return null;
        }

        if (currentEffect != null &&
            currentEffect.gameObject != null &&
            currentEffect.gameObject.activeInHierarchy)
        {
            return currentEffect;
        }

        currentEffect = CS_EffectFactory.CreateEffect(
            effectPrefab,
            position,
            rotation,
            parent);

        if (currentEffect == null)
        {
            return null;
        }

        currentEffect.SetOnEffectEndAction(DestroyEffect);

        CSST_EffectPlayData playData = new CSST_EffectPlayData();
        playData.CSST_EffectPlayData_Init();

        playData.SetPosition(position);
        playData.SetRotation(rotation);

        currentEffect.PlayEffect(playData);

        return currentEffect;
    }

    public static void EndAndClear(
        ref CSAD_EffectCommonProcessBase currentEffect)
    {
        if (currentEffect == null)
        {
            return;
        }

        currentEffect.EndEffect();
        currentEffect = null;
    }

    private static void DestroyEffect(CSAD_EffectCommonProcessBase effect)
    {
        if (effect == null)
        {
            return;
        }

        Object.Destroy(effect.gameObject);
    }
}
