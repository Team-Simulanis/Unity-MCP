using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace com.IvanMurzak.Unity.MCP.Editor.Tests
{
    public partial class TestToolGameObject
    {
        const string GO_ParentName = "root";
        const string GO_Child1Name = "child1";
        const string GO_Child2Name = "child2";

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
            DestroyAllGameObjectsInActiveScene();
            yield return null;
        }
        public static void DestroyAllGameObjectsInActiveScene()
        {
            var scene = SceneManager.GetActiveScene();
            foreach (var go in scene.GetRootGameObjects())
                Object.DestroyImmediate(go);
        }
        void ResultValidation(string result)
        {
            Debug.Log($"[{nameof(TestToolGameObject)}] Result:\n{result}");
            Assert.IsNotNull(result, $"Result should not be empty or null.");
            Assert.IsFalse(result.ToLower().Contains("error"), $"Result should not contain 'error'.\n{result}");
        }
    }
}