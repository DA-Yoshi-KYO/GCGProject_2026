using UnityEngine;
using System.Collections;

public class RandomShake : CS_TimeLineActionBase
{
    [SerializeField] float power = 0.1f;

    Vector3 defaultPos;

    private void Awake()
    {
        defaultPos = transform.localPosition;
    }

    public override void PlayAction()
    {
        StopAllCoroutines();
        StartCoroutine(Action());
    }

    public override IEnumerator Action()
    {
        float timer = 0;

        while (timer < duration)
        {
            Vector3 newPos = defaultPos + Random.insideUnitSphere * power;
            transform.localPosition = new Vector3(newPos.x, defaultPos.y, newPos.z);

            timer += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = defaultPos;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(new Vector3(0, 0, 5), ForceMode.Impulse);
            rb.useGravity = true;
        }
    }
}
