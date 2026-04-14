using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// ZombieAI — ذكاء الزومبي الكامل.
/// يتجول، يحس باللاعب، يلاحقه، يهاجمه، ويتكلم!
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class ZombieAI : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRange = 20f;
    public float attackRange    = 1.5f;

    [Header("Stats")]
    public float health       = 100f;
    public float attackDamage = 10f;
    public float attackCooldown = 1.2f;

    [Header("Speed")]
    public float wanderSpeed = 1.5f;
    public float chaseSpeed  = 4f;

    [Header("Dialogue")]
    public float talkInterval = 5f;

    // ── internal ──────────────────────────────────────────────
    private NavMeshAgent _agent;
    private Transform    _player;
    private float        _attackTimer;
    private float        _talkTimer;
    private float        _wanderTimer;
    private bool         _isDead;

    private enum State { Wander, Chase, Attack }
    private State _state = State.Wander;

    // جمل الزومبي بالعربي 😄
    private readonly string[] _lines =
    {
        "بررررر... دماغ!!!",
        "لا تهرب... أنا بس عايز العشا!",
        "جعان جداً... مشيييي!",
        "الدماغ... الدماغ... الدماغ...",
        "إنت فين يا إنسان؟!",
        "رائحتك وصلتلي من بعيييد...",
        "مش هتفلت منيييي!",
        "أنا زومبي ذكي... أعرف أفكر!",
        "جاي عليييك يا صاحبي!",
        "هههههه... مفيش فرار!",
    };

    // ── Lifecycle ──────────────────────────────────────────────
    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        _talkTimer  = Random.Range(1f, talkInterval);
        _wanderTimer = 0f;
        // Try to find player immediately
        FindPlayer();
        // Also keep trying every second in case player spawns later
        InvokeRepeating(nameof(FindPlayer), 0f, 1f);
    }

    void Update()
    {
        if (_isDead) return;
        UpdateState();
        HandleDialogue();
    }

    // ── State Machine ──────────────────────────────────────────
    void UpdateState()
    {
        if (_player == null) { Wander(); return; }

        float dist = Vector3.Distance(transform.position, _player.position);

        if (dist <= attackRange)
        {
            _state = State.Attack;
            AttackTarget();
        }
        else if (dist <= detectionRange)
        {
            _state = State.Chase;
            ChaseTarget();
        }
        else
        {
            _state = State.Wander;
            Wander();
        }
    }

    void ChaseTarget()
    {
        _agent.isStopped = false;
        _agent.speed     = chaseSpeed;
        _agent.SetDestination(_player.position);
    }

    void AttackTarget()
    {
        _agent.isStopped = true;
        transform.LookAt(new Vector3(_player.position.x, transform.position.y, _player.position.z));

        _attackTimer -= Time.deltaTime;
        if (_attackTimer <= 0f)
        {
            _attackTimer = attackCooldown;
            _player.GetComponent<PlayerHealth>()?.TakeDamage(attackDamage);
        }
    }

    void Wander()
    {
        _agent.isStopped = false;
        _agent.speed     = wanderSpeed;
        _wanderTimer    -= Time.deltaTime;

        if (_wanderTimer <= 0f || !_agent.hasPath || _agent.remainingDistance < 0.5f)
        {
            _wanderTimer = Random.Range(3f, 7f);
            Vector3 dest = RandomNavPoint(transform.position, 15f);
            _agent.SetDestination(dest);
        }
    }

    // ── Dialogue ───────────────────────────────────────────────
    void HandleDialogue()
    {
        _talkTimer -= Time.deltaTime;
        if (_talkTimer <= 0f)
        {
            _talkTimer = talkInterval + Random.Range(-1f, 2f);
            string line = _lines[Random.Range(0, _lines.Length)];
            Debug.Log($"🧟 [{name}]: {line}");
            // Show floating text above zombie
            StartCoroutine(ShowFloatingText(line));
        }
    }

    IEnumerator ShowFloatingText(string text)
    {
        // Simple world-space label using OnGUI isn't ideal,
        // but for now we log — you can attach a world-space Canvas prefab here
        yield return null;
    }

    // ── Health ─────────────────────────────────────────────────
    public void TakeDamage(float dmg)
    {
        if (_isDead) return;
        health -= dmg;

        // Flash red briefly
        StartCoroutine(FlashOnHit());

        if (health <= 0f) Die();
    }

    IEnumerator FlashOnHit()
    {
        var rend = GetComponentInChildren<Renderer>();
        if (rend == null) yield break;
        Color orig = rend.material.color;
        rend.material.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        if (rend) rend.material.color = orig;
    }

    void Die()
    {
        _isDead = true;
        _agent.isStopped = true;

        // Shrink and disappear
        StartCoroutine(DeathAnimation());
        ZombieSpawner.Instance?.OnZombieDied();
    }

    IEnumerator DeathAnimation()
    {
        float t = 0f;
        Vector3 startScale = transform.localScale;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t / 0.5f);
            yield return null;
        }
        Destroy(gameObject);
    }

    // ── Helpers ────────────────────────────────────────────────
    void FindPlayer()
    {
        if (_player != null) return;
        var go = GameObject.FindGameObjectWithTag("Player");
        if (go != null) _player = go.transform;
    }

    Vector3 RandomNavPoint(Vector3 origin, float radius)
    {
        for (int i = 0; i < 15; i++)
        {
            Vector3 rnd = Random.insideUnitSphere * radius + origin;
            rnd.y = origin.y;
            if (NavMesh.SamplePosition(rnd, out NavMeshHit hit, radius, NavMesh.AllAreas))
                return hit.position;
        }
        return origin;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
