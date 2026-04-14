// GameBootstrap.cs
// ضع هذا الملف في Assets/Scripts
// ثم اعمل Empty GameObject في الـ Scene واسمه "Bootstrap" وأضف هذا الـ Script عليه
// هيبني كل اللعبة تلقائياً — أرض، لاعب، زومبي، كاميرا، UI، كل حاجة!

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AI;
#endif

public class GameBootstrap : MonoBehaviour
{
    void Awake()
    {
        BuildScene();
    }

    void BuildScene()
    {
        // ── Lighting ───────────────────────────────────────────
        RenderSettings.ambientLight = new Color(0.1f, 0.05f, 0.05f);
        var sun = new GameObject("Sun").AddComponent<Light>();
        sun.type      = LightType.Directional;
        sun.color     = new Color(1f, 0.8f, 0.6f);
        sun.intensity = 0.8f;
        sun.transform.rotation = Quaternion.Euler(45, 60, 0);

        // ── Ground ─────────────────────────────────────────────
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(10, 1, 10); // 100x100
        var groundMat = new Material(Shader.Find("Standard"));
        groundMat.color = new Color(0.15f, 0.12f, 0.1f);
        ground.GetComponent<Renderer>().material = groundMat;
        ground.isStatic = true;

        // ── Walls (so zombies don't fall off) ─────────────────
        CreateWall(new Vector3(0, 2, 50),  new Vector3(100, 4, 1));
        CreateWall(new Vector3(0, 2, -50), new Vector3(100, 4, 1));
        CreateWall(new Vector3(50, 2, 0),  new Vector3(1, 4, 100));
        CreateWall(new Vector3(-50, 2, 0), new Vector3(1, 4, 100));

        // ── Some obstacles for fun ─────────────────────────────
        CreateObstacle(new Vector3(10, 1, 10));
        CreateObstacle(new Vector3(-10, 1, 10));
        CreateObstacle(new Vector3(10, 1, -10));
        CreateObstacle(new Vector3(-10, 1, -10));
        CreateObstacle(new Vector3(20, 1, 0));
        CreateObstacle(new Vector3(-20, 1, 0));

        // ── Player ────────────────────────────────────────────
        GameObject player = CreatePlayer();

        // ── Spawn Points ──────────────────────────────────────
        Vector3[] spawnPositions = {
            new Vector3(45, 0, 45),
            new Vector3(-45, 0, 45),
            new Vector3(45, 0, -45),
            new Vector3(-45, 0, -45),
            new Vector3(45, 0, 0),
            new Vector3(-45, 0, 0),
            new Vector3(0, 0, 45),
            new Vector3(0, 0, -45),
        };

        Transform[] spawnPoints = new Transform[spawnPositions.Length];
        for (int i = 0; i < spawnPositions.Length; i++)
        {
            var sp = new GameObject($"SpawnPoint_{i}");
            sp.transform.position = spawnPositions[i];
            spawnPoints[i] = sp.transform;
        }

        // ── Zombie Prefab (runtime) ────────────────────────────
        GameObject zombiePrefab = CreateZombiePrefab();

        // ── ZombieSpawner ──────────────────────────────────────
        GameObject spawnerGO = new GameObject("ZombieSpawner");
        ZombieSpawner spawner = spawnerGO.AddComponent<ZombieSpawner>();
        spawner.zombiePrefab  = zombiePrefab;
        spawner.spawnPoints   = spawnPoints;

        // ── UI ────────────────────────────────────────────────
        CreateUI(player);

        // ── Camera ────────────────────────────────────────────
        SetupCamera(player.transform);

        Debug.Log("✅ ZombieWaves — Scene built successfully! اللعبة جاهزة!");
    }

    // ── Player ────────────────────────────────────────────────
    GameObject CreatePlayer()
    {
        // Body
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag  = "Player";
        player.transform.position = Vector3.zero + Vector3.up * 1f;

        // Color — blue player
        var mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.2f, 0.4f, 0.9f);
        player.GetComponent<Renderer>().material = mat;

        // CharacterController
        Destroy(player.GetComponent<CapsuleCollider>());
        var cc = player.AddComponent<CharacterController>();
        cc.height = 2f;
        cc.radius = 0.5f;

        // Gun barrel
        var barrel = new GameObject("GunBarrel");
        barrel.transform.SetParent(player.transform);
        barrel.transform.localPosition = new Vector3(0.5f, 0.5f, 1f);

        // Scripts
        var ctrl = player.AddComponent<PlayerController>();
        ctrl.gunBarrel = barrel.transform;

        var health = player.AddComponent<PlayerHealth>();

        return player;
    }

    // ── Zombie Prefab ─────────────────────────────────────────
    GameObject CreateZombiePrefab()
    {
        GameObject z = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        z.name = "ZombiePrefab";

        // Green zombie color
        var mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.2f, 0.5f, 0.15f);
        z.GetComponent<Renderer>().material = mat;

        // Eyes (red spheres)
        AddEye(z.transform, new Vector3(-0.15f, 0.8f, 0.45f));
        AddEye(z.transform, new Vector3( 0.15f, 0.8f, 0.45f));

        // NavMeshAgent
        var agent = z.AddComponent<NavMeshAgent>();
        agent.height = 2f;
        agent.radius = 0.4f;
        agent.speed  = 3.5f;

        // ZombieAI script
        z.AddComponent<ZombieAI>();

        // Disable — spawner will instantiate copies
        z.SetActive(false);

        return z;
    }

    void AddEye(Transform parent, Vector3 localPos)
    {
        var eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        eye.transform.SetParent(parent);
        eye.transform.localPosition = localPos;
        eye.transform.localScale    = Vector3.one * 0.12f;
        var mat = new Material(Shader.Find("Standard"));
        mat.color = Color.red;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", Color.red * 2f);
        eye.GetComponent<Renderer>().material = mat;
        Destroy(eye.GetComponent<Collider>());
    }

    // ── Obstacles ─────────────────────────────────────────────
    void CreateObstacle(Vector3 pos)
    {
        GameObject obs = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obs.name = "Obstacle";
        obs.transform.position   = pos;
        obs.transform.localScale = new Vector3(2, 2, 2);
        obs.isStatic = true;
        var mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.3f, 0.2f, 0.1f);
        obs.GetComponent<Renderer>().material = mat;
    }

    void CreateWall(Vector3 pos, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "Wall";
        wall.transform.position   = pos;
        wall.transform.localScale = scale;
        wall.isStatic = true;
        var mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.25f, 0.2f, 0.2f);
        wall.GetComponent<Renderer>().material = mat;
    }

    // ── UI ────────────────────────────────────────────────────
    void CreateUI(GameObject player)
    {
        Canvas canvas = new GameObject("Canvas").AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.gameObject.AddComponent<CanvasScaler>();
        canvas.gameObject.AddComponent<GraphicRaycaster>();

        // Wave text (top center)
        Text waveText = CreateText(canvas.transform, "WaveText",
            "الموجة 1", 28, Color.white,
            new Vector2(0, -30), new Vector2(400, 50), new Vector2(0.5f, 1f));

        // Message text (center)
        Text msgText = CreateText(canvas.transform, "MessageText",
            "", 22, Color.yellow,
            new Vector2(0, 80), new Vector2(500, 50), new Vector2(0.5f, 0.5f));

        // Health bar
        Slider healthBar = CreateHealthBar(canvas.transform);

        // Ammo text (bottom right)
        Text ammoText = CreateText(canvas.transform, "AmmoText",
            "طلقات: 30/30", 20, Color.white,
            new Vector2(-80, 40), new Vector2(200, 40), new Vector2(1f, 0f));

        // Wave Banner Panel
        GameObject bannerPanel = new GameObject("WaveBannerPanel");
        bannerPanel.transform.SetParent(canvas.transform, false);
        var bannerImg = bannerPanel.AddComponent<Image>();
        bannerImg.color = new Color(0, 0, 0, 0.7f);
        var bannerRect = bannerPanel.GetComponent<RectTransform>();
        bannerRect.anchorMin = new Vector2(0.1f, 0.4f);
        bannerRect.anchorMax = new Vector2(0.9f, 0.6f);
        bannerRect.offsetMin = Vector2.zero;
        bannerRect.offsetMax = Vector2.zero;

        Text bannerText = CreateText(bannerPanel.transform, "BannerText",
            "الموجة 1 — استعد!", 36, Color.red,
            Vector2.zero, new Vector2(0, 0), new Vector2(0.5f, 0.5f));
        bannerPanel.SetActive(false);

        // Game Over Panel
        GameObject gameOverPanel = CreateGameOverPanel(canvas.transform);

        // UIManager
        var uiMgr = new GameObject("UIManager").AddComponent<UIManager>();
        uiMgr.waveText        = waveText;
        uiMgr.messageText     = msgText;
        uiMgr.waveBannerPanel = bannerPanel;
        uiMgr.waveBannerText  = bannerText;
        uiMgr.gameOverPanel   = gameOverPanel;

        // Link health bar to player
        player.GetComponent<PlayerHealth>().healthBar = healthBar;

        // Ammo text link (simple approach via PlayerController)
        // PlayerController will call UIManager.ShowMessage for ammo
    }

    Text CreateText(Transform parent, string name, string content, int size,
                    Color color, Vector2 anchoredPos, Vector2 sizeDelta, Vector2 anchor)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Text t = go.AddComponent<Text>();
        t.text      = content;
        t.fontSize  = size;
        t.color     = color;
        t.alignment = TextAnchor.MiddleCenter;
        t.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = sizeDelta == Vector2.zero ? new Vector2(400, 60) : sizeDelta;
        return t;
    }

    Slider CreateHealthBar(Transform parent)
    {
        GameObject sliderGO = new GameObject("HealthBar");
        sliderGO.transform.SetParent(parent, false);
        Slider slider = sliderGO.AddComponent<Slider>();
        slider.value = 1f;

        var rect = sliderGO.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
        rect.anchoredPosition = new Vector2(160, 40);
        rect.sizeDelta = new Vector2(300, 30);

        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(sliderGO.transform, false);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0, 0);
        var bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = bgRect.offsetMax = Vector2.zero;

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderGO.transform, false);
        var faRect = fillArea.AddComponent<RectTransform>();
        faRect.anchorMin = Vector2.zero;
        faRect.anchorMax = Vector2.one;
        faRect.offsetMin = faRect.offsetMax = Vector2.zero;

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.8f, 0.1f, 0.1f);
        var fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = fillRect.offsetMax = Vector2.zero;

        slider.fillRect = fillRect;
        slider.targetGraphic = fillImg;

        return slider;
    }

    GameObject CreateGameOverPanel(Transform parent)
    {
        GameObject panel = new GameObject("GameOverPanel");
        panel.transform.SetParent(parent, false);
        var img = panel.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.85f);
        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;

        CreateText(panel.transform, "GameOverText", "💀 انتهت اللعبة!", 48,
            Color.red, new Vector2(0, 60), new Vector2(500, 80), new Vector2(0.5f, 0.5f));

        CreateText(panel.transform, "FinalWaveText", "", 28,
            Color.white, new Vector2(0, -10), new Vector2(400, 50), new Vector2(0.5f, 0.5f));

        panel.SetActive(false);
        return panel;
    }

    // ── Camera ────────────────────────────────────────────────
    void SetupCamera(Transform playerTransform)
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            cam = camGO.AddComponent<Camera>();
            camGO.AddComponent<AudioListener>();
        }
        cam.gameObject.AddComponent<ThirdPersonCamera>().target = playerTransform;
    }
}
