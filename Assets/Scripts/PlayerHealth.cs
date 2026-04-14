using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// PlayerHealth — صحة اللاعب مع تأثير بصري لما بيتضرب.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;

    [Header("UI")]
    public Slider healthBar;

    private float _health;
    private bool  _isDead;

    // Flash overlay — created at runtime
    private Image _flashImage;

    void Start()
    {
        _health = maxHealth;
        UpdateBar();

        // Create damage flash overlay
        var canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            var go = new GameObject("DamageFlash");
            go.transform.SetParent(canvas.transform, false);
            _flashImage = go.AddComponent<Image>();
            _flashImage.color = new Color(1, 0, 0, 0f);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }

    public void TakeDamage(float dmg)
    {
        if (_isDead) return;
        _health = Mathf.Clamp(_health - dmg, 0, maxHealth);
        UpdateBar();

        if (_flashImage != null) StartCoroutine(Flash());

        if (_health <= 0f) Die();
    }

    void UpdateBar()
    {
        if (healthBar) healthBar.value = _health / maxHealth;
    }

    IEnumerator Flash()
    {
        _flashImage.color = new Color(1, 0, 0, 0.4f);
        yield return new WaitForSeconds(0.2f);
        _flashImage.color = new Color(1, 0, 0, 0f);
    }

    void Die()
    {
        _isDead = true;
        UIManager.Instance?.ShowGameOver();

        // Disable player input
        var ctrl = GetComponent<PlayerController>();
        if (ctrl) ctrl.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }
}
