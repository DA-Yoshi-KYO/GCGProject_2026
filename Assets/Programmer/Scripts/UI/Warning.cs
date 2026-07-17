using UnityEngine;

public class Warning : MonoBehaviour
{
    [SerializeField] private GameObject warningTextObject;
    [SerializeField] private float warningDuration = 2f;

    private float warningTimer = 0f;

    private void Awake()
    {
        if (warningTextObject == null)
        {
            Debug.LogError("Warning Text Object is not assigned.");
            return;
        }
        warningTextObject.SetActive(false);
    }

    private void Update()
    {
        if (warningTimer > 0f)
        {
            warningTimer -= Time.deltaTime;
            if (warningTimer <= 0f)
            {
                HideWarning();
            }
        }
    }

    public void ShowWarning()
    {
        if (warningTextObject == null)
        {
            Debug.LogWarning("Warning Text Object is not assigned.", this);
            return;
        }

        warningTextObject.SetActive(true);
        warningTimer = Mathf.Max(0f, warningDuration);

        if (warningTimer <= 0f)
        {
            HideWarning();
        }
    }

    private void HideWarning()
    {
        if (warningTextObject != null)
        {
            warningTextObject.SetActive(false);
        }
    }
}
