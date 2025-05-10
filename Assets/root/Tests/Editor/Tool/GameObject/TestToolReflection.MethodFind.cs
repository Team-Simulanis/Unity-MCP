using System.Collections;
using com.IvanMurzak.Unity.MCP.Common.Data.Unity;
using com.IvanMurzak.Unity.MCP.Editor.API;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace com.IvanMurzak.Unity.MCP.Editor.Tests
{
    public partial class TestToolReflection
    {
        void ResultValidation(string result)
        {
            Assert.IsFalse(result.Contains("[Error]"), $"[Error] {result}");
            Assert.IsTrue(result.Contains("[Success]"), $"[Success] {result}");
        }
        [UnityTest]
        public IEnumerator MethodFind_Transform()
        {
            var methodInfo = typeof(UnityEngine.Transform).GetMethod(nameof(UnityEngine.Transform.LookAt));

            var result = new Tool_Reflection().MethodFind(
                filter: new MethodPointerRef(methodInfo),
                knownNamespace: true,
                classNameMatchLevel: 6,
                methodNameMatchLevel: 6,
                parametersMatchLevel: 2);

            ResultValidation(result);
            yield return null;
        }
    }
}