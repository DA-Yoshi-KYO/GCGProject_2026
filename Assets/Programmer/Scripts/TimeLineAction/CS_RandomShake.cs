using UnityEngine;
using System.Collections;

public class RandomShake : MonoBehaviour
{
    [SerializeField] float power = 0.1f;
    [SerializeField] float duration = 1.0f;

    Vector3 defaultPos;

    private void Awake()
    {
        defaultPos = transform.localPosition;
    }

    public void PlayShake()
    {
        StopAllCoroutines();
        StartCoroutine(Shake());
    }

    IEnumerator Shake()
    {
        float timer = 0;

        while (timer < duration)
        {
            transform.localPosition =
                defaultPos + Random.insideUnitSphere * power;

            timer += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = defaultPos;
    }
}
