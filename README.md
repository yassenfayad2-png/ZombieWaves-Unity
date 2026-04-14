# 🧟 ZombieWaves — Unity Endless Zombie Survival

لعبة زومبي لا تنتهي مبنية على Unity مع ذكاء اصطناعي حقيقي للأعداء.

---

## 📁 ملفات المشروع

| ملف | الوظيفة |
|-----|---------|
| `ZombieAI.cs` | ذكاء الزومبي — يطارد / يهاجم / يتكلم |
| `ZombieSpawner.cs` | يولّد موجات لا نهائية من الزومبي |
| `PlayerController.cs` | حركة اللاعب والإطلاق |
| `PlayerHealth.cs` | صحة اللاعب |
| `Bullet.cs` | الرصاصة وتأثيرها على الزومبي |
| `UIManager.cs` | واجهة المستخدم — موجات، game over |

---

## 🚀 طريقة التثبيت في Unity

1. **افتح Unity Hub** وأنشئ مشروع جديد بـ 3D template.
2. **حمّل الملفات** من GitHub وضعها في `Assets/Scripts/`.
3. **أضف NavMesh** للأرض:
   - اختار الأرض → Window → AI → Navigation → Bake.
4. **اعمل Zombie Prefab**:
   - Capsule أو model → أضف `NavMeshAgent` + `ZombieAI` + `Animator`.
5. **اعمل Player**:
   - Capsule → أضف `CharacterController` + `PlayerController` + `PlayerHealth`.
6. **اعمل ZombieSpawner**:
   - Empty GameObject → أضف `ZombieSpawner` → اسحب Zombie Prefab وSpawn Points.
7. **اعمل UIManager**:
   - Canvas → أضف `UIManager` → وصّل النصوص والـ panels.

---

## 🤖 نظام الذكاء الاصطناعي

### ZombieAI — State Machine
```
Wander → (لما يحس باللاعب) → Chase → (لما يوصل) → Attack
```

- **Wander:** يتمشى عشوائي على الـ NavMesh.
- **Chase:** يجري ناحية اللاعب بسرعة أعلى.
- **Attack:** يضرب اللاعب كل 1.2 ثانية.
- **Dialogue:** كل 5 ثواني يقول جملة عربية مضحكة 😄

### ZombieSpawner — Endless Waves
- الموجة 1: 5 زومبي
- الموجة 2: 8 زومبي (أقوى وأسرع)
- الموجة 3: 11 زومبي... وهكذا إلى الأبد!

---

## 🎮 Controls

| زر | الفعل |
|----|-------|
| WASD | حركة |
| Mouse | كاميرا |
| Left Click | إطلاق |
| R | إعادة تعبئة |
| Space | قفز |

---

صُنع بواسطة **ياسين** 🎮 — أشبال مصر الرقمية
