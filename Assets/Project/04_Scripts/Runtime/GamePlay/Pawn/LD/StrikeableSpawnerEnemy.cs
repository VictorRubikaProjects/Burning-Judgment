using System;
using PrimeTween;
using UnityEngine;

public class StrikeableSpawnerEnemy : StrikeableElement, IDamageable
{
    [Header("Spawner Settings")]
    [SerializeField] protected int life;
    [SerializeField] protected float frequency;
    [SerializeField] protected float range;
    [Space]
    [SerializeField] protected EnemyBase[] enemies;
    [SerializeField] protected LayerMask layerGround;
    [Space]
    [SerializeField] protected MeshRenderer rendererVisual;
    [SerializeField] private Transform visual;
    
    private FrequencyTimer m_frequencyTimer;
    
    private bool m_isDestroyed;
    private bool m_isOnTargetPoint = false;
    
    private Material m_materialInstance;

    private int m_maxLife;

    private float m_targetHeight = 2f;

    private Color m_colorMaxLife = Color.green;
    private Color m_colorMinLife = Color.red;

    private Tween m_currentTween; 

    private void Kill()
    {
        if (m_isDestroyed) return;
        m_isDestroyed = true;
        Destroy(gameObject);
    }

    private void Awake()
    {
        m_frequencyTimer = new FrequencyTimer(1f/frequency);
        
        m_frequencyTimer.OnTick += SpawnEnemy;
        
        m_frequencyTimer.Reset();
        m_frequencyTimer.Start();
        
        if (rendererVisual)
        {
            m_materialInstance = new Material(rendererVisual.material);
            rendererVisual.material = m_materialInstance;
        }
        
        m_maxLife = life;
        
        UpdateColorLife();
        
        
        onStruck.AddListener(TakeDamage);
    }

    private void Start()
    {
        float startY = visual.localPosition.y;
        float endY = startY + m_targetHeight;

        m_currentTween = Tween.LocalPositionY(
            visual,
            startValue: startY,
            endValue: endY,
            duration: 1f,
            cycles: -1,
            cycleMode: CycleMode.Yoyo,
            ease: Ease.InOutSine 
        );
    }

    private void SpawnEnemy()
    {
        Debug.Log("CC");
        
        if (m_isDestroyed) return;

        if (enemies == null || enemies.Length == 0)
        {
            Debug.LogWarning("StrikeableSpawnerEnemy: No enemies assigned to spawn.", this);
            return;
        }

        int randomIndex = UnityEngine.Random.Range(0, enemies.Length);
        EnemyBase enemy = Instantiate(enemies[randomIndex]);
        enemy.gameObject.SetActive(false);

        bool findGoodPlacement = false;
        int securityCount = 100;
        Vector3 spawnPosition = Vector3.zero;

        while (!findGoodPlacement)
        {
            if (securityCount <= 0) break;
            
            securityCount--;
            
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * range;
            Vector3 rayStart = transform.position + new Vector3(randomCircle.x, range, randomCircle.y);

            if (!Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, range * 2f, layerGround)) continue;

            float offset = 0.1f;
            
            spawnPosition = hit.point + new Vector3(0, (enemy.StatsBase.Height / 2) + offset, 0);
            
            findGoodPlacement = true;
        }

        if (!findGoodPlacement)
        {
            Destroy(enemy.gameObject);
            return;
        }

        enemy.transform.position = spawnPosition;
        
        if (enemy.TransformCache != null)
        {
            enemy.TransformCache.position = spawnPosition;
        }
        
        enemy.SetupPlayerRef(ServiceLocator.Get<GameService>().Player);
        enemy.gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        if (m_frequencyTimer != null)
        {
            m_frequencyTimer.OnTick -= SpawnEnemy; 
            m_frequencyTimer.Stop();
            m_frequencyTimer.Dispose();
        }
        
        if (m_materialInstance != null)
        {
            Destroy(m_materialInstance);
            m_materialInstance = null;
        }

        m_currentTween.Stop();
        
        onStruck.RemoveListener(TakeDamage);
    }

    public void TakeDamage()
    {
        if (!CanTakeDamage())
        {
            Kill();
            return;
        }
        
        life = Mathf.Max(0, life - 1);
        UpdateColorLife();
    }

    public bool CanTakeDamage() => life > 0;

    private void UpdateColorLife()
    {
        if (m_materialInstance != null && m_maxLife > 0)
        {
            m_materialInstance.color = Color.Lerp(m_colorMinLife, m_colorMaxLife, life / (float)m_maxLife);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.aquamarine;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}