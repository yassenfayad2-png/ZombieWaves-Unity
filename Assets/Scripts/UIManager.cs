using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// UIManager — يتحكم في كل الواجهة: موجات، رسائل، game over.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Texts")]
    public Text waveText;
    public Text messageText;

    [Header("Panels")]
    public GameObject waveBannerPanel;
    public Text       waveBannerText;
    public GameObject gameOverPanel;
    public Text       finalWaveText;

    private int   _lastWave;
    private Coroutine _msgCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ShowWaveBanner(int wave)
    {
        _lastWave = wave;
        if (waveText)       waveText.text      = $"الموجة {wave}";
        if (waveBannerText) waveBannerText.text = $"⚡ الموجة {wave} — استعد!";
        if (waveBannerPanel) StartCoroutine(BannerRoutine());
    }

    IEnumerator BannerRoutine()
    {
        waveBannerPanel.SetActive(true);
        yield return new WaitForSeconds(3f);
        waveBannerPanel.SetActive(false);
    }

    public void ShowMessage(string msg)
    {
        if (!messageText) return;
        if (_msgCoroutine != null) StopCoroutine(_msgCoroutine);
        messageText.text = msg;
        _msgCoroutine = StartCoroutine(ClearAfter(3f));
    }

    IEnumerator ClearAfter(float sec)
    {
        yield return new WaitForSeconds(sec);
        if (messageText) messageText.text = "";
    }

    public void ShowGameOver()
    {
        if (gameOverPanel) gameOverPanel.SetActive(true);
        if (finalWaveText) finalWaveText.text = $"وصلت للموجة {_lastWave} 💪";
        Time.timeScale = 0f;
    }

    // زر إعادة اللعب
    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
