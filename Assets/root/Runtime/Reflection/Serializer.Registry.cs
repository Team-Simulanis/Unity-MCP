using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace com.IvanMurzak.Unity.MCP.Utils
{
    /// <summary>
    /// Serializes Unity components to JSON format.
    /// </summary>
    public static partial class Serializer
    {
        public static class Registry
        {
            static ConcurrentBag<IReflectionSerializer> _serializers = new();
            static Registry()
            {
                // Basics
                Add(new RS_Primitive());
                Add(new RS_Generic<object>());
                Add(new RS_Array());

                // Components
                Add(new RS_UnityEngineObject());
                Add(new RS_UnityEngineComponent());
                Add(new RS_UnityEngineTransform());
                Add(new RS_UnityEngineRenderer());
                Add(new RS_UnityEngineMeshFilter());

                // Assets
                Add(new RS_UnityEngineMaterial());
                Add(new RS_UnityEngineSprite());
            }

            public static void Add(IReflectionSerializer serializer)
            {
                if (serializer == null)
                    return;

                _serializers.Add(serializer);
            }
            public static void Remove<T>() where T : IReflectionSerializer
            {
                var serializer = _serializers.FirstOrDefault(s => s is T);
                if (serializer == null)
                    return;

                _serializers = new ConcurrentBag<IReflectionSerializer>(_serializers.Where(s => s != serializer));
            }

            public static IReadOnlyList<IReflectionSerializer> GetAllSerializers() => _serializers.ToList();

            static IEnumerable<IReflectionSerializer> FindRelevantSerializers(Type type) => _serializers
                    .Select(s => (s, s.SerializationPriority(type)))
                    .Where(s => s.Item2 > 0)
                    .OrderByDescending(s => s.Item2)
                    .Select(s => s.s);

            public static IEnumerable<IReflectionSerializer> BuildSerializersChain(Type type)
            {
                var serializers = FindRelevantSerializers(type);
                foreach (var serializer in serializers)
                {
                    yield return serializer;
                    if (!serializer.AllowCascadeSerialize)
                        break;
                }
            }
            public static IEnumerable<IReflectionSerializer> BuildPopulatorsChain(Type type)
            {
                var serializers = FindRelevantSerializers(type);
                foreach (var serializer in serializers)
                {
                    yield return serializer;
                    if (!serializer.AllowCascadePopulate)
                        break;
                }
            }
        }
    }
}
