using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// UIManager — shows wave banners, messages, and game-over screen.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    public Text  waveText;          // "الموجة 3"
    public Text  messageText;       // temporary messages
    public float messageDuration = 3f;

    [Header("Wave Banner")]
    public GameObject waveBannerPanel;
    public Text       waveBannerText;

    [Header("Game Over")]
    public GameObject gameOverPanel;
    public Text       finalWaveText;

    private int _lastWave;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ShowWaveBanner(int wave)
    {
        _lastWave = wave;
        if (waveText)       waveText.text       = $"الموجة {wave}";
        if (waveBannerText) waveBannerText.text  = $"الموجة {wave} — استعد!";
        if (waveBannerPanel) StartCoroutine(ShowThenHide(waveBannerPanel, 3f));
    }

    public void ShowMessage(string msg)
    {
        if (!messageText) return;
        messageText.text = msg;
        StartCoroutine(ClearMessage());
    }

    System.Collections.IEnumerator ClearMessage()
    {
        yield return new WaitForSeconds(messageDuration);
        if (messageText) messageText.text = "";
    }

    System.Collections.IEnumerator ShowThenHide(GameObject panel, float seconds)
    {
        panel.SetActive(true);
        yield return new WaitForSeconds(seconds);
        panel.SetActive(false);
    }

    public void ShowGameOver()
    {
        if (gameOverPanel) gameOverPanel.SetActive(true);
        if (finalWaveText) finalWaveText.text = $"وصلت للموجة {_lastWave}!";
        Time.timeScale = 0f;  // pause
    }

    // Called by Restart button
    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
