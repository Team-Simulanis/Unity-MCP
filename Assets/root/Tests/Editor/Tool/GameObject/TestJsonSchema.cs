using System;
using System.Collections;
using System.Globalization;
using System.Text.Json.Nodes;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Common.Data.Utils;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace com.IvanMurzak.Unity.MCP.Editor.Tests
{
    public class TestJsonSchema : BaseTest
    {
        static void ValidateType<T>() => ValidateType(typeof(T));
        static void ValidateType(Type type)
        {
            var schema = JsonUtils.GetSchema(type);
            Assert.IsNotNull(schema, $"Schema for '{type.FullName}' is null");

            var typeNodes = JsonUtils.FindAllProperties(schema, "type");
            foreach (var typeNode in typeNodes)
            {
                switch (typeNode)
                {
                    case JsonValue value:
                        var typeValue = value.ToString();
                        Assert.IsFalse(string.IsNullOrEmpty(typeValue), $"Type node for '{type.FullName}' is empty");
                        Assert.IsFalse(typeValue == "null", $"Type node for '{type.FullName}' is \"null\" string");
                        break;
                    default:
                        Assert.Fail($"Unexpected type node for '{type.FullName}': {typeNode}");
                        break;
                }
            }
        }

        [UnityTest]
        public IEnumerator Primitives()
        {
            ValidateType<int>();
            ValidateType<float>();
            ValidateType<bool>();
            ValidateType<string>();
            ValidateType<CultureTypes>(); // enum

            yield return null;
        }

        [UnityTest]
        public IEnumerator Classes()
        {
            // ValidateType<object>();
            ValidateType<ObjectRef>();

            yield return null;
        }

        [UnityTest]
        public IEnumerator Structs()
        {
            ValidateType<DateTime>();

            yield return null;
        }

        [UnityTest]
        public IEnumerator UnityStructs()
        {
            ValidateType<UnityEngine.Vector3>();
            ValidateType<UnityEngine.Vector3Int>();
            ValidateType<UnityEngine.Vector2>();
            ValidateType<UnityEngine.Vector2Int>();

            yield return null;
        }

        [UnityTest]
        public IEnumerator Unity()
        {
            ValidateType<UnityEngine.Object>();
            ValidateType<UnityEngine.Rigidbody>();
            ValidateType<UnityEngine.Animation>();
            ValidateType<UnityEngine.Material>();
            ValidateType<UnityEngine.Transform>();
            ValidateType<UnityEngine.SpriteRenderer>();
            ValidateType<UnityEngine.MeshRenderer>();

            yield return null;
        }
    }
}