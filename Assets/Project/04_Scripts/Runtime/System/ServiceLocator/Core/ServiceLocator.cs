using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class ServiceLocator 
{
    /// <summary>
    /// Store services
    /// </summary>
    private static readonly Dictionary<Type,object> _services = new();
    
    /// <summary>
    /// Let iterate on services without giving access to modification
    /// </summary>
    public static IEnumerable<Type> Services => _services.Keys;

    public static bool IsRegistered<T>() where T : IGameService => _services.ContainsKey(typeof(T));
    
    public static bool IsEmpty => _services.Count == 0;
    
    public static int Count => _services.Count;
    
    /// <summary>
    /// Unity Editor cleaning of statics elements
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetServicesStatics()
    {
        foreach (var service in _services.Values)
        {
            if (service is IGameService gameService)
                gameService.ShutDownService();
        }
        _services.Clear();
    }

    #region Register

    /// <summary>
    /// Register Service into Locator
    /// </summary>
    /// <param name="service"></param>
    /// <typeparam name="T"></typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    public static async UniTask Register<T>(T service) where T : IGameService
    {
        if (service == null)
        {
            throw new ArgumentNullException(nameof(service), $"[RegisterLocator.Register] service cannot be null.");
        }
        
        Type type = typeof(T);
        await RegisterInternal(type, service);
    }
    
    public static async UniTask Register(Type type, object service)
    {
        if (service == null)
        {
            throw new ArgumentNullException(nameof(service), "[ServiceLocator.Register] service cannot be null.]");
        }

        if (type == null)
        {
            throw new ArgumentNullException(nameof(type), "[ServiceLocator.Register] service type cannot be null.]");
        }
        
        if (!type.IsInstanceOfType(service))
        {
            throw new ArgumentException(
                $"[ServiceLocator.Register] : Service of type {service.GetType().FullName} does not match requested type {type.FullName}.",
                nameof(service));
        }
        
        if (!typeof(IGameService).IsAssignableFrom(type))
        {
            throw new ArgumentException($"[ServiceLocator.Register] : type {type.FullName} does not implement IGameService.",
                nameof(service));
        }
        
        await RegisterInternal(type, service);
    }

    /// <summary>
    /// Helper methods for Register 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="service"></param>
    private static async UniTask RegisterInternal(Type type, object service)
    {
        if (_services.ContainsKey(type))
            throw new ArgumentException($"[ServiceLocator.Register] service type {service.GetType().FullName} already registered.");
        
        if (service is IGameService gameService) await gameService.Initialize();
        
        _services[type] = service;
    }

    #endregion

    #region Get

    private static object GetInternal(Type type)
    {
        if (!typeof(IGameService).IsAssignableFrom(type))
            throw new ArgumentException(
                $"[ServiceLocator.Get] : Service {type.FullName} not found.");
        
        if (_services.TryGetValue(type, out object service)) return service;

        throw new ArgumentException(
            $"[ServiceLocator.Get] : Service {type.FullName} not found. Make sure all services are registered before accessing through the service locator.");
    }

    public static T Get<T>() where T : IGameService
    {
        Type type = typeof(T);
        return (T)GetInternal(type);
    }

    public static object Get(Type type)
    {
        return type == null ? 
            throw new ArgumentNullException(nameof(type), "[ServiceLocator.Get] : Type cannot be null.") :
            GetInternal(type);
    }

    public static bool TryGet<T>(out T service) where T : IGameService
    {
        Type type = typeof(T);

        if (_services.TryGetValue(type, out object obj))
        {
            service = (T)obj;
            return true;
        }

        service = default;
        return false;
    }

    #endregion

    #region Unregister

    public static void Unregister(Type type)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type), "[ServiceLocator.Unregister] type cannot be null.");
        }
        
        UnregisterInternal(type);
    }

    public static void Unregister<T>() where T : IGameService
    {
        Type type = typeof(T);
        Unregister(type);
    }
    
    private static void UnregisterInternal(Type type)
    {
        if (!typeof(IGameService).IsAssignableFrom(type))
        {
            throw new ArgumentException($"[ServiceLocator.Unregister] type {type.FullName} must implement IGameService.");
        }

        if (_services.TryGetValue(type, out object obj))
        {
            IGameService service = (IGameService)obj;
            service.ShutDownService();
        }
        
        _services.Remove(type);
    }

    #endregion
}