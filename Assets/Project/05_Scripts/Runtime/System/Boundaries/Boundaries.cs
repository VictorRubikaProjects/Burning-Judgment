using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Boundaries : MonoBehaviour
{
    [SerializeField] private List<Transform> boundariesTargets = new();

    [Serializable]
    struct BoundsData
    {
        public Vector3 center;
        public Vector3 size;
    }
    
    [SerializeField] private BoundsData boundsData;

    private void Update()
    {
        ApplyBoundaries();
    }

    private void ApplyBoundaries()
    {
        foreach (Transform target in boundariesTargets)
        {
            float x = target.position.x;
            float y = target.position.y;
            float z = target.position.z;
            
            x = CalculateBoundaries(x, boundsData.center.x, boundsData.size.x);
            y = CalculateBoundaries(y, boundsData.center.y, boundsData.size.y);
            z = CalculateBoundaries(z, boundsData.center.z, boundsData.size.z);
            
            Vector3 finalPosition = new Vector3(x, y, z);
            
            target.position = finalPosition;
        }
    }

    private float CalculateBoundaries(float posAxes,float centerAxes,float sizeAxes)
    {
        return Mathf.Clamp(posAxes, centerAxes - sizeAxes/2, centerAxes + sizeAxes/2);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(boundsData.center, boundsData.size);
    }
}
