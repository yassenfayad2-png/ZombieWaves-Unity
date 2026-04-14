using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// PlayerHealth — handles the player taking damage from zombies.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    private float _health;

    [Header("UI")]
    public Slider healthBar;   // drag Health slider here in Inspector
    public Image  damageFlash; // full-screen red image (alpha 0 normally)

    private bool _isDead;

    void Start()
    {
        _health = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(float damage)
    {
        if (_isDead) return;
        _health -= damage;
        _health  = Mathf.Clamp(_health, 0, maxHealth);
        UpdateHealthBar();

        if (damageFlash) StartCoroutine(FlashRed());

        if (_health <= 0f) Die();
    }

    void UpdateHealthBar()
    {
        if (healthBar) healthBar.value = _health / maxHealth;
    }

    System.Collections.IEnumerator FlashRed()
    {
        if (!damageFlash) yield break;
        damageFlash.color = new Color(1, 0, 0, 0.35f);
        yield return new WaitForSeconds(0.15f);
        damageFlash.color = new Color(1, 0, 0, 0f);
    }

    void Die()
    {
        _isDead = true;
        UIManager.Instance?.ShowGameOver();
        // Disable player controls
        GetComponent<UnityEngine.InputSystem.PlayerInput>()?.DeactivateInput();
    }
}
