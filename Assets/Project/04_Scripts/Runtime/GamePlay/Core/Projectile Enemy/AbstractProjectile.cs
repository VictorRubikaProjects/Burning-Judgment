using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Event_Bus;
using UnityEngine;

public abstract class AbstractProjectile : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] protected float m_speed = 10f;
    [SerializeField] protected float m_lifetime = 5f;
    [SerializeField] protected int m_damage = 1;

    protected Vector3 m_direction;
    protected string m_poolKey;
    protected EnemyShooter m_owner;

    private CancellationTokenSource m_ctsLifetime;
    private bool m_isActive;

    public virtual void Launch(Vector3 direction, string poolKey, EnemyShooter owner)
    {
        m_owner = owner;
        m_direction = direction.normalized;
        m_poolKey = poolKey;
        m_isActive = true;

        CancelLifetime();
        m_ctsLifetime = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);

        ExpireAsync(m_ctsLifetime.Token).Forget();
    }

    protected virtual void Update()
    {
        if (!m_isActive) return;

        transform.position += m_direction * (m_speed * Time.deltaTime);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!m_isActive) return;
        if (!TryHandleHit(other)) return;

        ReturnToPool();
    }

    protected abstract bool TryHandleHit(Collider other);

    private async UniTaskVoid ExpireAsync(CancellationToken token)
    {
        bool isCancelled = await UniTask.Delay(TimeSpan.FromSeconds(m_lifetime), cancellationToken: token).SuppressCancellationThrow();

        if (isCancelled) return;

        ReturnToPool();
    }

    protected virtual void ReturnToPool()
    {
        if (!m_isActive) return;

        m_isActive = false;
        CancelLifetime();

        ObjectPooler.EnqueueObject(this, m_poolKey);
    }

    protected virtual void OnDestroy()
    {
        CancelLifetime();
    }

    private void CancelLifetime()
    {
        if (m_ctsLifetime == null) return;

        m_ctsLifetime.Cancel();
        m_ctsLifetime.Dispose();
        m_ctsLifetime = null;
    }
}