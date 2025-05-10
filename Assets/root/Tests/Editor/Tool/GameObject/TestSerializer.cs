using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Common.Data.Utils;
using com.IvanMurzak.Unity.MCP.Common.Reflection;
using com.IvanMurzak.Unity.MCP.Common.Reflection.Convertor;
using com.IvanMurzak.Unity.MCP.Reflection.Convertor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace com.IvanMurzak.Unity.MCP.Editor.Tests
{
    public partial class TestSerializer
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            Debug.Log($"[{nameof(TestToolGameObject)}] SetUp");
            yield return null;
        }
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Debug.Log($"[{nameof(TestToolGameObject)}] TearDown");
            yield return null;
        }

        static void PrintSerializers<TTarget>()
        {
            Debug.Log($"Serialize <b>[{typeof(TTarget)}]</b> priority:\n" + string.Join("\n", Reflector.Instance.Convertors.GetAllSerializers()
                .Select(x => $"{x.GetType()}: Priority: {x.SerializationPriority(typeof(TTarget))}")
                .ToList()));
        }
        static void TestSerializerChain<TTarget, TSerializer>(int countOfSerializers)
        {
            PrintSerializers<TTarget>();
            TestSerializerChain(typeof(TTarget), typeof(TSerializer), countOfSerializers);

            PrintSerializers<IEnumerable<TTarget>>();
            TestSerializerChain(typeof(IEnumerable<TTarget>), typeof(RS_ArrayUnity), countOfSerializers);

            PrintSerializers<List<TTarget>>();
            TestSerializerChain(typeof(List<TTarget>), typeof(RS_ArrayUnity), countOfSerializers);

            PrintSerializers<TTarget[]>();
            TestSerializerChain(typeof(TTarget[]), typeof(RS_ArrayUnity), countOfSerializers);

            Debug.Log($"-------------------------------------------");
        }
        static void TestSerializerChain(Type type, Type serializerType, int countOfSerializers)
        {
            var serializers = Reflector.Instance.Convertors.BuildSerializersChain(type).ToList();
            Assert.AreEqual(countOfSerializers, serializers.Count, $"{type}: Only {countOfSerializers} serializer should be used.");
            Assert.AreEqual(serializerType, serializers[0].GetType(), $"{type}: The first serializer should be {serializerType}.");
        }
        static void TestPopulatorChain<TTarget, TSerializer>(int countOfSerializers)
        {
            PrintSerializers<TTarget>();
            TestPopulatorChain(typeof(TTarget), typeof(TSerializer), countOfSerializers);

            PrintSerializers<IEnumerable<TTarget>>();
            TestPopulatorChain(typeof(IEnumerable<TTarget>), typeof(RS_ArrayUnity), countOfSerializers);

            PrintSerializers<List<TTarget>>();
            TestPopulatorChain(typeof(List<TTarget>), typeof(RS_ArrayUnity), countOfSerializers);

            PrintSerializers<TTarget[]>();
            TestPopulatorChain(typeof(TTarget[]), typeof(RS_ArrayUnity), countOfSerializers);

            Debug.Log($"-------------------------------------------");
        }
        static void TestPopulatorChain(Type type, Type serializerType, int countOfSerializers)
        {
            var serializers = Reflector.Instance.Convertors.BuildPopulatorsChain(type).ToList();
            Assert.AreEqual(countOfSerializers, serializers.Count, $"{type.Name}: Only {countOfSerializers} serializer should be used.");
            Assert.AreEqual(serializerType, serializers[0].GetType(), $"{type.Name}: The first serializer should be {serializerType.Name}.");
        }

        [UnityTest]
        public IEnumerator RS_SerializersOrder()
        {
            TestSerializerChain<int, RS_Primitive>(1);
            TestSerializerChain<float, RS_Primitive>(1);
            TestSerializerChain<bool, RS_Primitive>(1);
            TestSerializerChain<string, RS_Primitive>(1);
            TestSerializerChain<DateTime, RS_Primitive>(1);
            TestSerializerChain<CultureTypes, RS_Primitive>(1); // enum
            TestSerializerChain<object, RS_Generic<object>>(1);
            TestSerializerChain<ObjectRef, RS_Generic<object>>(1);

            TestSerializerChain<UnityEngine.Object, RS_UnityEngineObject>(1);
            TestSerializerChain<UnityEngine.Rigidbody, RS_UnityEngineComponent>(1);
            TestSerializerChain<UnityEngine.Animation, RS_UnityEngineComponent>(1);
            TestSerializerChain<UnityEngine.Material, RS_UnityEngineMaterial>(1);
            TestSerializerChain<UnityEngine.Transform, RS_UnityEngineTransform>(1);
            TestSerializerChain<UnityEngine.SpriteRenderer, RS_UnityEngineRenderer>(1);
            TestSerializerChain<UnityEngine.MeshRenderer, RS_UnityEngineRenderer>(1);

            yield return null;
        }

        [UnityTest]
        public IEnumerator SerializeMaterial()
        {
            var material = new Material(Shader.Find("Standard"));

            var serialized = Reflector.Instance.Serialize(material);
            var json = JsonUtils.Serialize(serialized);
            Debug.Log($"[{nameof(TestSerializer)}] Result:\n{json}");

            var glossinessValue = 1.0f;
            var colorValue = new Color(1.0f, 0.0f, 0.0f, 0.5f);

            serialized.SetPropertyValue("_Glossiness", glossinessValue);
            serialized.SetPropertyValue("_Color", colorValue);

            var objMaterial = (object)material;
            Reflector.Instance.Populate(ref objMaterial, serialized);

            Assert.AreEqual(glossinessValue, material.GetFloat("_Glossiness"), 0.001f, $"Material property '_Glossiness' should be {glossinessValue}.");
            Assert.AreEqual(colorValue, material.GetColor("_Color"), $"Material property '_Glossiness' should be {glossinessValue}.");

            yield return null;
        }


        [UnityTest]
        public IEnumerator SerializeMaterialArray()
        {
            var material1 = new Material(Shader.Find("Standard"));
            var material2 = new Material(Shader.Find("Standard"));

            var materials = new[] { material1, material2 };

            var serialized = Reflector.Instance.Serialize(materials);
            var json = JsonUtils.Serialize(serialized);
            Debug.Log($"[{nameof(TestSerializer)}] Result:\n{json}");

            // var glossinessValue = 1.0f;
            // var colorValue = new Color(1.0f, 0.0f, 0.0f, 0.5f);

            // serialized.SetPropertyValue("_Glossiness", glossinessValue);
            // serialized.SetPropertyValue("_Color", colorValue);

            // var objMaterial = (object)material;
            // Serializer.Populate(ref objMaterial, serialized);

            // Assert.AreEqual(glossinessValue, material.GetFloat("_Glossiness"), 0.001f, $"Material property '_Glossiness' should be {glossinessValue}.");
            // Assert.AreEqual(colorValue, material.GetColor("_Color"), $"Material property '_Glossiness' should be {glossinessValue}.");

            yield return null;
        }
    }
}