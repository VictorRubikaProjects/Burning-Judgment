using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestionnaire de pool d'objets pour optimiser l'instanciation et la destruction d'objets Unity.
/// </summary>
public static class ObjectPooler
{
    /// <summary>
    /// Dictionnaire de référence des prefabs par nom de pool.
    /// </summary>
    public static Dictionary<string, Component> PoolLookUp = new Dictionary<string, Component>();
    
    /// <summary>
    /// Dictionnaire contenant les files d'objets disponibles pour chaque pool.
    /// </summary>
    public static Dictionary<string, Queue<Component>> PoolDictionary = new Dictionary<string, Queue<Component>>();
    
    /// <summary>
    /// Remet un objet dans le pool et le désactive.
    /// </summary>
    /// <param name="item">L'objet à remettre dans le pool</param>
    /// <param name="name">Le nom du pool</param>
    public static void EnqueueObject<T>(T item, string name) where T : Component
    {
        if (item == null) return;

        if (!PoolDictionary.ContainsKey(name))
        {
            Debug.LogError($"Le pool {name} n'existe pas !");
            return;
        }

        item.transform.position = Vector3.zero;
        item.gameObject.SetActive(false);

        PoolDictionary[name].Enqueue(item);
    }
    
    /// <summary>
    /// Récupère un objet du pool. Si le pool est vide, crée une nouvelle instance.
    /// </summary>
    /// <param name="key">Le nom du pool</param>
    /// <returns>L'objet récupéré du pool</returns>
    public static T DequeueObject<T>(string key) where T : Component
    {
        if (!PoolDictionary.ContainsKey(key)) {
            Debug.LogError($"Le pool {key} n'existe pas !");
            return null;
        }
        
        while (PoolDictionary[key].Count > 0)
        {
            if (PoolDictionary[key].TryDequeue(out var item) && item != null)
            {
                return (T)item;
            }
        }

        return CreateNewInstance((T)PoolLookUp[key]);
    }
    
    private static T CreateNewInstance<T>(T prefab) where T : Component
    {
        T newInstance = Object.Instantiate(prefab);
        newInstance.gameObject.SetActive(false);
        return newInstance;
    }

    /// <summary>
    /// Crée une nouvelle instance et l'ajoute au pool.
    /// </summary>
    public static T EnqueueNewInstance<T>(T item, string key) where T : Component
    {
        if (!PoolDictionary.ContainsKey(key))
        {
            Debug.LogError($"Le pool {key} n'existe pas !");
            return null;
        }

        T newInstance = Object.Instantiate(item);
        newInstance.gameObject.SetActive(false);
        newInstance.transform.position = Vector3.zero;
        PoolDictionary[key].Enqueue(newInstance);
        return newInstance;
    }
    
    /// <summary>
    /// Initialise un nouveau pool avec un nombre déterminé d'objets pré-instanciés.
    /// </summary>
    /// <param name="pooledItemPrefab">Le prefab à pooler</param>
    /// <param name="poolSize">Le nombre d'instances à créer</param>
    /// <param name="dictionaryEntry">Le nom du pool</param>
    public static void SetupPool<T>(T pooledItemPrefab, int poolSize, string dictionaryEntry) where T : Component
    {
        if (PoolDictionary.ContainsKey(dictionaryEntry))
        {
            while (PoolDictionary[dictionaryEntry].Count > 0)
            {
                Component leftover = PoolDictionary[dictionaryEntry].Dequeue();
                if (leftover != null) Object.Destroy(leftover.gameObject);
            }

            PoolLookUp[dictionaryEntry] = pooledItemPrefab;
        }
        else
        {
            PoolDictionary.Add(dictionaryEntry, new Queue<Component>());
            PoolLookUp.Add(dictionaryEntry, pooledItemPrefab);
        }

        for (int i = 0; i < poolSize; i++)
        {
            T pooledInstance = Object.Instantiate(pooledItemPrefab);
            pooledInstance.gameObject.SetActive(false);
            PoolDictionary[dictionaryEntry].Enqueue(pooledInstance);
        }
    }
}