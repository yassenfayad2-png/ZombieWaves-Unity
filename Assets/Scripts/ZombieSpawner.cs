using System.Collections;
using UnityEngine;

/// <summary>
/// ZombieSpawner — endlessly spawns zombies in waves.
/// Each wave is harder than the last (more zombies, faster, more health).
/// </summary>
public class ZombieSpawner : MonoBehaviour
{
    public static ZombieSpawner Instance { get; private set; }

    [Header("Prefabs & Points")]
    public GameObject zombiePrefab;
    public Transform[] spawnPoints;       // drag spawn positions here in Inspector

    [Header("Wave Settings")]
    public int   baseZombiesPerWave  = 5;
    public float timeBetweenWaves    = 8f;
    public float spawnInterval       = 0.8f;  // delay between each spawn in a wave

    [Header("Scaling per Wave")]
    public int   extraZombiesPerWave = 3;     // how many more zombies each wave
    public float healthScalePerWave  = 20f;   // extra HP per wave
    public float speedScalePerWave   = 0.2f;  // extra speed per wave

    // ── internal ──────────────────────────────────────────────
    private int   _currentWave      = 0;
    private int   _zombiesAlive     = 0;
    private int   _zombiesToSpawn   = 0;
    private bool  _waveInProgress   = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(GameLoop());
    }

    // ── Main Loop ──────────────────────────────────────────────
    IEnumerator GameLoop()
    {
        while (true)   // endless game — never stops!
        {
            _currentWave++;
            UIManager.Instance?.ShowWaveBanner(_currentWave);

            yield return new WaitForSeconds(3f); // show banner for 3 s

            yield return StartCoroutine(SpawnWave(_currentWave));

            // Wait until all zombies in this wave are dead
            yield return new WaitUntil(() => _zombiesAlive <= 0);

            UIManager.Instance?.ShowMessage("الموجة انتهت! استعد...");
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    IEnumerator SpawnWave(int wave)
    {
        _waveInProgress = true;
        _zombiesToSpawn  = baseZombiesPerWave + (wave - 1) * extraZombiesPerWave;
        _zombiesAlive    = _zombiesToSpawn;

        float extraHealth = (wave - 1) * healthScalePerWave;
        float extraSpeed  = (wave - 1) * speedScalePerWave;

        for (int i = 0; i < _zombiesToSpawn; i++)
        {
            SpawnOneZombie(extraHealth, extraSpeed);
            yield return new WaitForSeconds(spawnInterval);
        }

        _waveInProgress = false;
    }

    void SpawnOneZombie(float extraHealth, float extraSpeed)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points assigned to ZombieSpawner!");
            return;
        }

        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject go   = Instantiate(zombiePrefab, point.position, point.rotation);

        ZombieAI ai = go.GetComponent<ZombieAI>();
        if (ai != null)
        {
            ai.health     += extraHealth;
            ai.chaseSpeed += extraSpeed;
            ai.wanderSpeed = Mathf.Min(ai.wanderSpeed + extraSpeed * 0.3f, 3f);
        }
    }

    // Called by ZombieAI.Die()
    public void OnZombieDied()
    {
        _zombiesAlive = Mathf.Max(0, _zombiesAlive - 1);
    }

    // ── Gizmos ─────────────────────────────────────────────────
    void OnDrawGizmos()
    {
        if (spawnPoints == null) return;
        Gizmos.color = Color.cyan;
        foreach (var p in spawnPoints)
            if (p) Gizmos.DrawWireSphere(p.position, 0.5f);
    }
}
