using System.Collections.Generic;
using UnityEngine;

namespace Helpers.Runtime.Physics
{
    public static class PhysicsHelper
    {
        private static readonly Collider[] _internalBuffer = new Collider[128];
        private static readonly RaycastHit[] s_hitBuffer = new RaycastHit[16];

        private static readonly IComparer<RaycastHit> s_distanceComparer =
            Comparer<RaycastHit>.Create((a, b) => a.distance.CompareTo(b.distance));

        /// <summary>
        /// Détecte les objets dans un rayon donné en utilisant un buffer interne (Zéro Alloc).
        /// Revoie le nombre d'objets trouvés. Les résultats sont accessibles via le tableau partagé.
        /// </summary>
        public static int GetObjectsInRadius(Vector3 position, float radius, LayerMask layerMask, out Collider[] sharedBuffer)
        {
            sharedBuffer = _internalBuffer;
            return UnityEngine.Physics.OverlapSphereNonAlloc(position, radius, _internalBuffer, layerMask);
        }

        /// <summary>
        /// Permet à l'utilisateur de passer son propre tableau s'il préfère.
        /// </summary>
        public static int GetObjectsInRadiusNonAlloc(this Collider[] customBuffer, Vector3 position, float radius, LayerMask layerMask)
        {
            return UnityEngine.Physics.OverlapSphereNonAlloc(position, radius, customBuffer, layerMask);
        }
        
        public static Transform FindElementInDirectionByLayer(
            Vector3 position,
            float radius,
            Vector3 direction,
            float distance,
            LayerMask targetLayerMask,
            LayerMask obstacleLayerMask)
        {
            LayerMask combinedMask = targetLayerMask | obstacleLayerMask;

            if (!UnityEngine.Physics.SphereCast(position, radius, direction, out RaycastHit hit, distance, combinedMask))
                return null;

            int hitLayerBit = 1 << hit.collider.gameObject.layer;

            if ((obstacleLayerMask.value & hitLayerBit) != 0) return null;

            return (targetLayerMask.value & hitLayerBit) != 0 ? hit.collider.transform : null;
        }
        
        public static bool TryFindElementInDirection<T>(
            Vector3 position,
            float radius,
            Vector3 direction,
            float distance,
            LayerMask castMask,
            out T target,
            out RaycastHit hit,
            Transform ignore = null) where T : class
        {
            target = null;
            hit = default;

            int count = UnityEngine.Physics.SphereCastNonAlloc(
                position,
                radius,
                direction,
                s_hitBuffer,
                distance,
                castMask);

            System.Array.Sort(s_hitBuffer, 0, count, s_distanceComparer);

            for (int i = 0; i < count; i++)
            {
                RaycastHit candidate = s_hitBuffer[i];

                if (ignore != null && candidate.collider.transform.IsChildOf(ignore)) continue;

                hit = candidate;
                target = candidate.collider.GetComponentInParent<T>();
                return target != null;
            }

            return false;
        }
    }
}