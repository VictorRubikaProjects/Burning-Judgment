using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DummyShooter : Dummy
{
    [Header("Shooting")]
    [SerializeField] private float frequencyShoot;
    [SerializeField] private DummyProjectile m_projectilePrefab;
    [SerializeField] private Transform m_shootPoint;
    [SerializeField] private int m_poolSize = 10;

    [Header("Targeting")]
    [SerializeField] private Transform m_player;
    [SerializeField] private float m_rotationSpeed = 5f;

    private const string ProjectilePoolKey = "DummyProjectile";

    private CountdownTimer m_timerShoot;

    protected override void Awake()
    {
        base.Awake();

        m_timerShoot = new CountdownTimer(frequencyShoot);
        m_timerShoot.Start();

        ObjectPooler.SetupPool(m_projectilePrefab, m_poolSize, ProjectilePoolKey);

        if (m_player == null)
        {
            m_player = GameObject.FindWithTag("Player")?.transform;
        }
    }

    private void OnEnable()
    {
        m_timerShoot.OnTimerStop += Shoot;
    }

    private void OnDisable()
    {
        m_timerShoot.OnTimerStop -= Shoot;
    }

    protected override void Update()
    {
        base.Update();
        FacePlayer();
    }

    public override void TakeDamage()
    {
        if (CanTakeDamage())
        {
            m_timerShoot.Pause();
        }
        
        base.TakeDamage();
    }

    protected override async UniTask RecoverAsync()
    {
        await base.RecoverAsync();

        m_timerShoot.Reset();
        m_timerShoot.Start();
    }

    private void FacePlayer()
    {
        if (m_player == null || !CanTakeDamage()) return;

        Vector3 direction = m_player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, m_rotationSpeed * Time.deltaTime);
    }

    private void Shoot()
    {
        DummyProjectile projectile = ObjectPooler.DequeueObject<DummyProjectile>(ProjectilePoolKey);

        projectile.transform.SetPositionAndRotation(m_shootPoint.position, m_shootPoint.rotation);
        projectile.gameObject.SetActive(true);
        projectile.Launch(m_shootPoint.forward, ProjectilePoolKey);

        m_timerShoot.Reset();
        m_timerShoot.Start();
    }
}