using System;
using System.Collections;
using System.Globalization;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Common.Data.Utils;
using com.IvanMurzak.Unity.MCP.Common.Reflection;
using com.IvanMurzak.Unity.MCP.Utils;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace com.IvanMurzak.Unity.MCP.Editor.Tests
{
    public class TestJsonSerialize : BaseTest
    {
        static void ValidateType<T>(T sourceValue)
        {
            var serializedValue = JsonUtils.Serialize(sourceValue);
            var deserializedValue = JsonUtils.Deserialize<T>(serializedValue);

            var areEqual = Reflector.Instance.AreEqual(sourceValue, deserializedValue);
            Assert.IsTrue(areEqual, $"Serialized and deserialized values do not match for type '{typeof(T).FullName}'");
        }

        [UnityTest]
        public IEnumerator Primitives()
        {
            ValidateType(100);
            ValidateType(0.23f);
            ValidateType(true);
            ValidateType("hello world");
            ValidateType(CultureTypes.SpecificCultures); // enum

            yield return null;
        }

        [UnityTest]
        public IEnumerator Classes()
        {
            var go = new GameObject("TestObject");
            ValidateType(go.ToObjectRef());

            yield return null;
        }

        [UnityTest]
        public IEnumerator Structs()
        {
            ValidateType(DateTime.Now);

            yield return null;
        }

        [UnityTest]
        public IEnumerator UnityStructs()
        {
            ValidateType(UnityEngine.Vector3.up);
            ValidateType(UnityEngine.Vector3Int.up);
            ValidateType(UnityEngine.Vector2.up);
            ValidateType(UnityEngine.Vector2Int.up);

            yield return null;
        }

        [UnityTest]
        public IEnumerator Unity()
        {
            var go = new GameObject("TestObject");
            ValidateType(go.AddComponent<UnityEngine.Rigidbody>().ToObjectRef());
            ValidateType(new UnityEngine.Animation().ToObjectRef());
            ValidateType(new UnityEngine.Material(Shader.Find("Standard")).ToObjectRef());
            ValidateType(go.transform.ToObjectRef());
            ValidateType(go.AddComponent<UnityEngine.SpriteRenderer>().ToObjectRef());
            ValidateType(go.AddComponent<UnityEngine.MeshRenderer>().ToObjectRef());

            yield return null;
        }
    }
}