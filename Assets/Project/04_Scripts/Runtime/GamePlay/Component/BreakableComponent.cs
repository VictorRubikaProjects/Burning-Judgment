using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class BreakableComponent : MonoBehaviour
{
    [SerializeField] private int _life = 1;
    
    [SerializeField] private float _explosionForce = 50;
    [SerializeField] private float _explosionRadius = 5f;
    [SerializeField] private float _upwardsModifier = 1f;
    
    private bool m_isIntact;
    
    [Space]
    [SerializeField] protected GameObject _intactMesh;
    [SerializeField] private GameObject _shatteredMesh;
    [SerializeField] protected float durationCleanUp = 1.5f;

    private readonly List<Rigidbody> m_shatteredRbChildren = new();
    private readonly List<Collider> m_shatteredColliderChildren = new();
    private readonly List<GameObject> m_shatteredMeshChildren = new();

    private readonly CancellationTokenSource m_cts = new();

    protected virtual void Awake()
    {
        if (_intactMesh)
            _intactMesh.SetActive(true);

        if (_shatteredMesh)
            _shatteredMesh.SetActive(false);

        m_isIntact = true;

        m_shatteredMeshChildren.Clear();
        m_shatteredRbChildren.Clear();
        m_shatteredColliderChildren.Clear();
    }
    
    public virtual void Interact(Vector3 contactPoint)
    {
        _life--;
        if (_life > 0) return;

        Break(contactPoint);
    }

    private void Break(Vector3 contactPoint)
    {
        if (!m_isIntact) return;
        m_isIntact = false;

        if (!_intactMesh)
            return;

        if (!_shatteredMesh)
        {
            Destroy(_intactMesh);
            return;
        }

        _intactMesh.SetActive(false);
        _shatteredMesh.SetActive(true);

        var rigidbodies = _shatteredMesh.GetComponentsInChildren<Rigidbody>();

        m_shatteredRbChildren.AddRange(rigidbodies);

        foreach (var rb in rigidbodies)
        {
            rb.AddExplosionForce(_explosionForce, contactPoint, _explosionRadius, _upwardsModifier);

            if (rb.TryGetComponent<Collider>(out var col))
                m_shatteredColliderChildren.Add(col);
        }

        ShatterPiecesCleanUp().Forget();
    }
    
    private async UniTask ShatterPiecesCleanUp()
    {
        var token = this.GetCancellationTokenOnDestroy();

        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(2f), cancellationToken: token);

            int count = m_shatteredRbChildren.Count;
            float elapsedTime = 0f;

            foreach (Rigidbody rb in m_shatteredRbChildren)
            {
                rb.isKinematic = true;
                rb.detectCollisions = false;
            }

            foreach (Collider col in m_shatteredColliderChildren)
                col.enabled = false;

            foreach (Rigidbody rb in m_shatteredRbChildren)
            {
                m_shatteredMeshChildren.Add(rb.gameObject);
                Destroy(rb);
            }

            while (elapsedTime < durationCleanUp)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / durationCleanUp;

                for (int i = 0; i < count; i++)
                {
                    var go = m_shatteredMeshChildren[i];
                    if (!go) continue;

                    go.transform.localScale = Vector3.Lerp(go.transform.localScale, Vector3.zero, t);
                }

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            for (int i = 0; i < count; i++)
                if (m_shatteredMeshChildren[i])
                    m_shatteredMeshChildren[i].transform.localScale = Vector3.zero;
        }
        catch (OperationCanceledException)
        {

        }
    }
    
    private void OnDestroy()
    {
        m_cts.Cancel();
        m_cts.Dispose();
    }
    
}