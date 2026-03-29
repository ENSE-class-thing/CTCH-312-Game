using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private TextMeshProUGUI interactionText;
    [SerializeField] private float displayTime = 2f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowMessage(string message)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ShowMessageRoutine(message));
    }

    private System.Collections.IEnumerator ShowMessageRoutine(string message)
    {
        interactionText.text = message;
        interactionText.gameObject.SetActive(true);

        yield return new WaitForSeconds(displayTime);

        interactionText.gameObject.SetActive(false);
    }
}