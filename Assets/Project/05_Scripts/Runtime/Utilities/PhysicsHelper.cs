using UnityEngine;

namespace Helpers.Runtime.Physics
{
    public static class PhysicsHelper
    {
        private static readonly Collider[] _internalBuffer = new Collider[128];

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
    }
}