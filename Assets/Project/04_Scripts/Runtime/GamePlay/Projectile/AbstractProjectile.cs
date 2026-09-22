using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class AbstractProjectile : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] protected float m_speed = 10f;
    [SerializeField] protected float m_lifetime = 5f;
    [SerializeField] protected int m_damage = 1;

    protected Vector3 m_direction;
    protected string m_poolKey;
    private CancellationTokenSource m_ctsLifetime;

    public virtual void Launch(Vector3 direction, string poolKey)
    {
        m_direction = direction.normalized;
        m_poolKey = poolKey;

        m_ctsLifetime?.Cancel();
        m_ctsLifetime = new CancellationTokenSource();

        ExpireAsync(m_ctsLifetime.Token).Forget();
    }

    protected virtual void Update()
    {
        transform.position += m_direction * (m_speed * Time.deltaTime);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!TryHandleHit(other)) return;
        
        m_ctsLifetime?.Cancel();
        ReturnToPool();
    }

    protected abstract bool TryHandleHit(Collider other);

    private async UniTask ExpireAsync(CancellationToken token)
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(m_lifetime), cancellationToken: token);

        ReturnToPool();
    }

    protected virtual void ReturnToPool()
    {
        ObjectPooler.EnqueueObject(this, m_poolKey);
    }
}