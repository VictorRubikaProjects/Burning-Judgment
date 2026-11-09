using UnityEngine;
using UnityEngine.AI;

namespace Helpers.Runtime.Gameplay
{
    public static class NavMeshHelper
    {
        private static readonly NavMeshPath _sharedPath = new NavMeshPath();
        
        public static bool GetRandomPoint(Vector3 center, float radius, out Vector3 result)
        {
            Vector3 positionBrute = Random.insideUnitSphere * radius;
            positionBrute += center;
            
            if (NavMesh.SamplePosition(positionBrute, out NavMeshHit hit, radius, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }

            result = Vector3.zero;
            return false;
        }

        public static bool IsPositionValid(Vector3 position, float tolerance = 0.1f)
        {
            return NavMesh.SamplePosition(position, out NavMeshHit hit, tolerance, NavMesh.AllAreas);
        }

        public static float GetPathDistance(this NavMeshAgent agent, Vector3 destination)
        {
            agent.CalculatePath(destination, _sharedPath);

            if (_sharedPath.status != NavMeshPathStatus.PathComplete) return -1f;

            float distance = 0f;
            
            for (int i = 0; i < _sharedPath.corners.Length - 1; i++)
            {
                distance += Vector3.Distance(_sharedPath.corners[i], _sharedPath.corners[i + 1]);
            }
            
            return distance;
        }
    }
}