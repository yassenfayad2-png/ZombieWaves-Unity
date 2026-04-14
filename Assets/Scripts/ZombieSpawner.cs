using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// ZombieSpawner — موجات لا نهائية من الزومبي.
/// كل موجة أكثر وأقوى وأسرع.
/// يشتغل تلقائياً — مش محتاج أي إعداد يدوي.
/// </summary>
public class ZombieSpawner : MonoBehaviour
{
    public static ZombieSpawner Instance { get; private set; }

    [Header("Zombie")]
    public GameObject zombiePrefab;
    public Transform[] spawnPoints;

    [Header("Wave Config")]
    public int   baseCount        = 5;
    public int   extraPerWave     = 3;
    public float timeBetweenWaves = 8f;
    public float spawnDelay       = 0.6f;

    [Header("Scaling")]
    public float extraHealthPerWave = 20f;
    public float extraSpeedPerWave  = 0.3f;

    // ── internal ──────────────────────────────────────────────
    private int  _wave        = 0;
    private int  _aliveCount  = 0;
    private bool _spawning    = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // Wait a moment for the scene to finish building, then start
        StartCoroutine(WaitAndStart());
    }

    IEnumerator WaitAndStart()
    {
        yield return new WaitForSeconds(1.5f);
        StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop()
    {
        while (true)
        {
            _wave++;
            UIManager.Instance?.ShowWaveBanner(_wave);

            yield return new WaitForSeconds(3f);

            yield return StartCoroutine(SpawnWave(_wave));

            // Wait for all zombies to die
            yield return new WaitUntil(() => _aliveCount <= 0);

            UIManager.Instance?.ShowMessage($"✅ الموجة {_wave} انتهت! استعد...");

            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    IEnumerator SpawnWave(int wave)
    {
        int count = baseCount + (wave - 1) * extraPerWave;
        _aliveCount = count;

        float extraHP    = (wave - 1) * extraHealthPerWave;
        float extraSpeed = (wave - 1) * extraSpeedPerWave;

        UIManager.Instance?.ShowMessage($"موجة {wave} — {count} زومبي قادمين!");

        for (int i = 0; i < count; i++)
        {
            SpawnOne(extraHP, extraSpeed);
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    void SpawnOne(float extraHP, float extraSpeed)
    {
        if (zombiePrefab == null)
        {
            Debug.LogError("ZombieSpawner: zombiePrefab is null!");
            return;
        }

        // Pick a random spawn point
        Vector3 pos;
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            pos = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
        }
        else
        {
            // Fallback: random edge of the arena
            float side = Random.value > 0.5f ? 40f : -40f;
            pos = new Vector3(side, 0, Random.Range(-40f, 40f));
        }

        // Make sure position is on NavMesh
        if (NavMesh.SamplePosition(pos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            pos = hit.position;

        GameObject zombie = Instantiate(zombiePrefab, pos, Quaternion.identity);
        zombie.SetActive(true);

        var ai = zombie.GetComponent<ZombieAI>();
        if (ai != null)
        {
            ai.health       += extraHP;
            ai.chaseSpeed   += extraSpeed;
            ai.wanderSpeed   = Mathf.Min(ai.wanderSpeed + extraSpeed * 0.3f, 3.5f);
        }
    }

    public void OnZombieDied()
    {
        _aliveCount = Mathf.Max(0, _aliveCount - 1);
    }

    void OnDrawGizmos()
    {
        if (spawnPoints == null) return;
        Gizmos.color = Color.cyan;
        foreach (var p in spawnPoints)
            if (p) Gizmos.DrawWireSphere(p.position, 1f);
    }
}
