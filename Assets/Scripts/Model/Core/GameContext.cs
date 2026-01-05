using System;
using System.Collections.Generic;
using SkiGame.Model.Terrain;
using UnityEngine;

namespace SkiGame.Model.Core
{
    public static class GameContext
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public static Map Map => Get<Map>();

        public static T Get<T>()
            where T : class
        {
            Type type = typeof(T);
            if (_services.TryGetValue(type, out object service))
            {
                return (T)service;
            }

            Debug.LogError(
                $"GameContext: Service of type '{type.Name}' accessed but is not registered."
            );
            return null;
        }

        public static void Register<T>(T service)
            where T : class
        {
            Type type = typeof(T);
            if (_services.ContainsKey(type))
            {
                Debug.LogWarning(
                    $"GameContext: Service of type '{type.Name}' is being overwritten!"
                );
            }
            _services[type] = service;
        }

        public static void Clear()
        {
            _services.Clear();
        }
    }
}
