using System.Collections;
using UnityEngine;

/// <summary>
/// PlayerController — حركة اللاعب وإطلاق النار.
/// WASD للحركة، Mouse للنظر، Click للإطلاق، R للتعبئة.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed  = 6f;
    public float gravity    = -20f;
    public float jumpHeight = 1.5f;

    [Header("Shooting")]
    public Transform gunBarrel;
    public float     bulletSpeed  = 35f;
    public float     fireRate     = 0.2f;
    public int       maxAmmo      = 30;
    public float     reloadTime   = 1.5f;
    public float     bulletDamage = 34f;

    // ── internal ──────────────────────────────────────────────
    private CharacterController _cc;
    private Camera              _cam;
    private Vector3             _velocity;
    private float               _fireTimer;
    private int                 _ammo;
    private bool                _reloading;

    void Start()
    {
        _cc   = GetComponent<CharacterController>();
        _cam  = Camera.main;
        _ammo = maxAmmo;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
        UpdateAmmoUI();
    }

    void Update()
    {
        Move();
        HandleShooting();

        // Escape to unlock cursor
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }
    }

    // ── Movement ───────────────────────────────────────────────
    void Move()
    {
        if (_cam == null) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 fwd   = _cam.transform.forward; fwd.y = 0; fwd.Normalize();
        Vector3 right = _cam.transform.right;   right.y = 0; right.Normalize();

        Vector3 move = (fwd * v + right * h).normalized;

        if (move.magnitude > 0.1f)
        {
            Quaternion target = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, 15f * Time.deltaTime);
        }

        if (_cc.isGrounded)
        {
            _velocity.y = -2f;
            if (Input.GetButtonDown("Jump"))
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        _velocity.y += gravity * Time.deltaTime;

        _cc.Move((move * moveSpeed + new Vector3(0, _velocity.y, 0)) * Time.deltaTime);
    }

    // ── Shooting ───────────────────────────────────────────────
    void HandleShooting()
    {
        _fireTimer -= Time.deltaTime;
        if (_reloading) return;

        if ((Input.GetKeyDown(KeyCode.R) || _ammo <= 0) && !_reloading)
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetButton("Fire1") && _fireTimer <= 0f && _ammo > 0)
            Shoot();
    }

    void Shoot()
    {
        _fireTimer = fireRate;
        _ammo--;
        UpdateAmmoUI();

        if (gunBarrel == null) return;

        // Raycast shoot — instant hit detection
        Ray ray = new Ray(gunBarrel.position, gunBarrel.forward);

        // Also use camera direction for accuracy
        if (Camera.main != null)
        {
            Ray camRay = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            if (Physics.Raycast(camRay, out RaycastHit hit, 100f))
            {
                hit.collider.GetComponent<ZombieAI>()?.TakeDamage(bulletDamage);

                // Visual bullet tracer
                StartCoroutine(BulletTracer(gunBarrel.position, hit.point));
            }
            else
            {
                StartCoroutine(BulletTracer(gunBarrel.position, gunBarrel.position + gunBarrel.forward * 50f));
            }
        }
    }

    IEnumerator BulletTracer(Vector3 from, Vector3 to)
    {
        // Draw a line for a brief moment
        float t = 0f;
        while (t < 0.05f)
        {
            Debug.DrawLine(from, to, Color.yellow, 0.05f);
            t += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator Reload()
    {
        _reloading = true;
        UIManager.Instance?.ShowMessage("⟳ بيعبي السلاح...");
        yield return new WaitForSeconds(reloadTime);
        _ammo      = maxAmmo;
        _reloading = false;
        UpdateAmmoUI();
    }

    void UpdateAmmoUI()
    {
        UIManager.Instance?.ShowMessage($"🔫 طلقات: {_ammo}/{maxAmmo}");
    }
}
