using UnityEngine;

namespace Helpers.Runtime.Math
{
    public static class TransformHelper
    {
        public static void SetX(this Transform transform, float x) =>
            transform.position = new Vector3(x, transform.position.y, transform.position.z);
        
        public static void SetY(this Transform transform, float y) =>
            transform.position = new Vector3(transform.position.x, y, transform.position.z);
        
        public static void SetZ(this Transform transform, float z) =>
            transform.position = new Vector3(transform.position.x, transform.position.y, z);

        public static bool IsFacing(this Transform origin, Transform target, float maxAngle) =>
        Vector3.Angle(origin.forward, (target.position - origin.position).normalized) < maxAngle;

        public static void LookAtSmooth(this Transform transform, Vector3 targetPosition, float speed)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speed * Time.deltaTime);
        }

        public static Transform GetClosestTransform(this Transform origin, Collider[] targets, int count)
        {
            if (targets == null || targets.Length == 0) return null;

            Transform result = null;
            float minDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                if (!targets[i]) continue;
                
                float distance = Vector3.SqrMagnitude(targets[i].transform.position - origin.position);

                if (!(distance < minDistance)) continue;
                minDistance = distance;
                result = targets[i].transform;
            }
            
            return result;
        }

        public static void ResetTransform(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
        
        
    }
    
}