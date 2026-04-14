using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// ZombieAI — controls zombie movement, detection, attacking, and AI dialogue.
/// Attach to a Zombie prefab that has a NavMeshAgent component.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class ZombieAI : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRange = 15f;
    public float attackRange = 1.8f;
    public LayerMask playerLayer;

    [Header("Stats")]
    public float health = 100f;
    public float attackDamage = 10f;
    public float attackCooldown = 1.2f;

    [Header("Speed")]
    public float wanderSpeed = 1.5f;
    public float chaseSpeed = 4f;

    [Header("Dialogue — AI Zombie Talk")]
    public float talkInterval = 5f;         // seconds between zombie lines
    public GameObject dialogueBubblePrefab; // optional: a world-space UI prefab

    // ── internal ──────────────────────────────────────────────
    private NavMeshAgent _agent;
    private Transform    _player;
    private float        _attackTimer;
    private float        _talkTimer;
    private float        _wanderTimer;
    private Vector3      _wanderTarget;
    private bool         _isDead;

    private enum State { Wander, Chase, Attack }
    private State _state = State.Wander;

    // Pre-written AI-style zombie dialogue lines
    private readonly string[] _zombieLines =
    {
        "بررررر... دماغ!!!",
        "لا تهرب... أنا بس عايز العشا!",
        "ما لقتش أكل من زمان...",
        "جعان جداً... مشيييي!",
        "الدماغ... الدماغ... الدماغ...",
        "إنت فين يا إنسان؟!",
        "رائحتك وصلتلي من بعيييد...",
        "مش هتفلت منيييي!",
    };

    // ── Unity lifecycle ────────────────────────────────────────
    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        _talkTimer  = Random.Range(0f, talkInterval); // stagger so not all zombies talk at once
        _wanderTimer = 0f;
        FindPlayer();
    }

    void Update()
    {
        if (_isDead) return;

        FindPlayer();
        UpdateState();
        HandleDialogue();
    }

    // ── State machine ──────────────────────────────────────────
    void UpdateState()
    {
        if (_player == null) { Wander(); return; }

        float dist = Vector3.Distance(transform.position, _player.position);

        if (dist <= attackRange)
        {
            _state = State.Attack;
            AttackPlayer();
        }
        else if (dist <= detectionRange)
        {
            _state = State.Chase;
            ChasePlayer();
        }
        else
        {
            _state = State.Wander;
            Wander();
        }
    }

    void ChasePlayer()
    {
        _agent.speed = chaseSpeed;
        _agent.SetDestination(_player.position);
    }

    void AttackPlayer()
    {
        _agent.ResetPath(); // stop moving while attacking
        transform.LookAt(_player);

        _attackTimer -= Time.deltaTime;
        if (_attackTimer <= 0f)
        {
            _attackTimer = attackCooldown;
            // Tell the player script to take damage
            _player.GetComponent<PlayerHealth>()?.TakeDamage(attackDamage);
        }
    }

    void Wander()
    {
        _agent.speed = wanderSpeed;
        _wanderTimer -= Time.deltaTime;

        if (_wanderTimer <= 0f || !_agent.hasPath)
        {
            _wanderTimer = Random.Range(3f, 6f);
            _wanderTarget = RandomNavPoint(transform.position, 10f);
            _agent.SetDestination(_wanderTarget);
        }
    }

    // ── Dialogue ───────────────────────────────────────────────
    void HandleDialogue()
    {
        _talkTimer -= Time.deltaTime;
        if (_talkTimer <= 0f)
        {
            _talkTimer = talkInterval + Random.Range(-1f, 2f);
            string line = _zombieLines[Random.Range(0, _zombieLines.Length)];
            SayLine(line);
        }
    }

    void SayLine(string line)
    {
        Debug.Log($"[Zombie {name}]: {line}");

        // If you have a dialogue bubble prefab, show it
        if (dialogueBubblePrefab != null)
        {
            var bubble = Instantiate(dialogueBubblePrefab,
                transform.position + Vector3.up * 2.5f,
                Quaternion.identity);
            bubble.GetComponentInChildren<UnityEngine.UI.Text>()?.SetText(line);
            Destroy(bubble, 3f);
        }
    }

    // ── Health ─────────────────────────────────────────────────
    public void TakeDamage(float damage)
    {
        if (_isDead) return;
        health -= damage;
        if (health <= 0f) Die();
    }

    void Die()
    {
        _isDead = true;
        _agent.isStopped = true;
        GetComponent<Animator>()?.SetTrigger("Die");
        // Notify the spawner so it can replace this zombie
        ZombieSpawner.Instance?.OnZombieDied();
        Destroy(gameObject, 3f);
    }

    // ── Helpers ────────────────────────────────────────────────
    void FindPlayer()
    {
        if (_player != null) return;
        var go = GameObject.FindGameObjectWithTag("Player");
        if (go) _player = go.transform;
    }

    Vector3 RandomNavPoint(Vector3 origin, float radius)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * radius + origin;
            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, radius, NavMesh.AllAreas))
                return hit.position;
        }
        return origin;
    }

    // ── Gizmos ─────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
