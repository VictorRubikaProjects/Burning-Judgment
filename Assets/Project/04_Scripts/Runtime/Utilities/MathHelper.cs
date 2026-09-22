using UnityEngine;

namespace Helpers.Runtime.Math
{
    /// <summary>
    /// Fonctions utilitaires pour les calculs mathématiques.
    /// </summary>
    public static class MathHelper
    {
        /// <summary>
        /// Convertit une valeur d'une plage vers une autre en conservant la proportion.
        /// </summary>
        /// <param name="value">La valeur à convertir</param>
        /// <param name="fromMin">Minimum de la plage d'origine</param>
        /// <param name="fromMax">Maximum de la plage d'origine</param>
        /// <param name="toMin">Minimum de la plage cible</param>
        /// <param name="toMax">Maximum de la plage cible</param>
        /// <returns>La valeur remappée dans la nouvelle plage</returns>
        public static float Remap(this float value, float fromMin, float fromMax, float toMin = 0, float toMax = 1)
        {
            float pourcentage = (value - fromMin) / (fromMax - fromMin);
            return Mathf.Lerp(toMin, toMax, pourcentage);
        }

        /// <summary>
        /// Vérifie si un point est dans une sphère définie par un centre et un rayon.
        /// </summary>
        /// <param name="point">Le point à tester</param>
        /// <param name="center">Le centre de la sphère</param>
        /// <param name="radius">Le rayon de la sphère</param>
        /// <returns>True si le point est dans le rayon</returns>
        public static bool IsInRadius(this Vector3 point, Vector3 center, float radius)
        {
            return Vector3.SqrMagnitude(point - center) < (radius * radius);
        }

        /// <summary>
        /// Calcule l'angle en degrés de la direction pointant de from vers to.
        /// </summary>
        /// <param name="from">Point de départ</param>
        /// <param name="to">Point d'arrivée</param>
        /// <returns>L'angle en degrés (0° = droite, 90° = haut)</returns>
        public static float GetAngle2D(Vector2 from, Vector2 to)
        {
            Vector2 direction = to - from;
            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// Génère une direction aléatoire normalisée en 2D.
        /// </summary>
        /// <returns>Un vecteur de longueur 1 dans une direction aléatoire</returns>
        public static Vector2 RandomDirection2D()
        {
            float rnd = Mathf.Deg2Rad * Random.Range(0, 360);
            return new Vector2(Mathf.Cos(rnd), Mathf.Sin(rnd));
        }
    }
}