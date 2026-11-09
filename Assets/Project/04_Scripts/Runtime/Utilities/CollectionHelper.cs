using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Helpers.Runtime.Collection
{
    /// <summary>
    /// Fonctions utilitaires pour manipuler les collections.
    /// </summary>
    public static class CollectionHelper
    {
        /// <summary>
        /// Mélange aléatoirement les éléments d'une liste en place (algorithme Fisher-Yates).
        /// </summary>
        /// <param name="list">La liste à mélanger</param>
        public static void Shuffle<T>(this List<T> list)
        {
            for (int i = 0; i < list.Count - 1; i++)
            {
                int random = Random.Range(i, list.Count);
                
                (list[i], list[random]) = (list[random], list[i]);
            }
        }

        /// <summary>
        /// Retourne un élément aléatoire de la liste avec une probabilité uniforme.
        /// </summary>
        /// <param name="list">La liste source</param>
        /// <returns>Un élément aléatoire</returns>
        /// <exception cref="Exception">Si la liste est vide</exception>
        public static T GetRandom<T>(this List<T> list)
        {
            if (list.Count == 0) throw new Exception("Liste vide !");
            
            return list[Random.Range(0, list.Count)];
        }

        /// <summary>
        /// Retourne un élément aléatoire de la liste en fonction des poids spécifiés.
        /// Les éléments avec un poids plus élevé ont plus de chances d'être choisis.
        /// </summary>
        /// <param name="list">La liste source</param>
        /// <param name="weights">Les poids associés à chaque élément</param>
        /// <returns>Un élément aléatoire pondéré</returns>
        /// <exception cref="Exception">Si les listes n'ont pas la même longueur</exception>
        public static T GetRandomWeighted<T>(this List<T> list, List<float> weights)
        {
            if (list.Count != weights.Count)throw new Exception("Les deux listes n'ont pas les mêmes longueurs !");

            float totalWeight = 0;
            
            for (int i = 0; i < weights.Count; i++) totalWeight += weights[i];
            
            float weightRnd = Random.Range(0, totalWeight);

            float cumul = 0;

            for (int i = 0; i < list.Count ; i++)
            {
                cumul += weights[i];

                if (cumul >= weightRnd) return list[i];
            }
            
            return list[^1];
        }
    }
}