using UnityEngine;

/// <summary>
/// PlayerController — simple third-person movement + shooting.
/// Requires: CharacterController, Camera tagged MainCamera.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed  = 6f;
    public float turnSpeed  = 10f;
    public float gravity    = -19.6f;
    public float jumpHeight = 1.5f;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform  gunBarrel;
    public float      bulletSpeed  = 30f;
    public float      fireRate     = 0.25f;   // seconds between shots
    public int        maxAmmo      = 30;
    public float      reloadTime   = 1.5f;

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
    }

    void Update()
    {
        Move();
        HandleShooting();
    }

    // ── Movement ───────────────────────────────────────────────
    void Move()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 camForward = _cam.transform.forward;
        Vector3 camRight   = _cam.transform.right;
        camForward.y = 0; camRight.y = 0;
        camForward.Normalize(); camRight.Normalize();

        Vector3 move = (camForward * v + camRight * h).normalized;

        if (move.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot,
                                                   turnSpeed * Time.deltaTime);
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

        if (Input.GetKey(KeyCode.R) || _ammo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetButton("Fire1") && _fireTimer <= 0f && _ammo > 0)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        if (!bulletPrefab || !gunBarrel) return;
        _fireTimer = fireRate;
        _ammo--;

        GameObject bullet = Instantiate(bulletPrefab, gunBarrel.position, gunBarrel.rotation);
        bullet.GetComponent<Rigidbody>()?.AddForce(gunBarrel.forward * bulletSpeed, ForceMode.VelocityChange);
        Destroy(bullet, 4f);

        UIManager.Instance?.ShowMessage($"طلقات: {_ammo}/{maxAmmo}");
    }

    System.Collections.IEnumerator Reload()
    {
        _reloading = true;
        UIManager.Instance?.ShowMessage("بيعبي السلاح...");
        yield return new WaitForSeconds(reloadTime);
        _ammo      = maxAmmo;
        _reloading = false;
        UIManager.Instance?.ShowMessage($"طلقات: {_ammo}/{maxAmmo}");
    }
}
