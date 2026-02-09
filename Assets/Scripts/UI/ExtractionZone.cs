using UnityEngine;
using TMPro;

public class ExtractionZone : MonoBehaviour
{
    public float extractTime = 5f;
    public TMP_Text timerText;

    float timer;
    bool playerInside;

    void Start()
    {
        if (timerText)
            timerText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!playerInside) return;

        timer += Time.deltaTime;

        if (timerText)
        {
            float left = Mathf.Clamp(extractTime - timer, 0, extractTime);
            timerText.text = $"Extracting: {left:0.0}";
        }

        if (timer >= extractTime)
        {
            Extract();
        }
    }

    void Extract()
    {
        Debug.Log("EXTRACTION COMPLETE");

        if (timerText)
            timerText.gameObject.SetActive(false);

        // TODO: finish game / load scene / disable player
        enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;
        timer = 0f;

        if (timerText)
            timerText.gameObject.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;
        timer = 0f;

        if (timerText)
            timerText.gameObject.SetActive(false);
    }
}