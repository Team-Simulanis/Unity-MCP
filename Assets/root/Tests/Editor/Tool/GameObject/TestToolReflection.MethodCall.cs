using System.Collections;
using com.IvanMurzak.Unity.MCP.Common.Data.Unity;
using com.IvanMurzak.Unity.MCP.Common.Reflection;
using com.IvanMurzak.Unity.MCP.Editor.API;
using UnityEngine.TestTools;

namespace com.IvanMurzak.Unity.MCP.Editor.Tests
{
    public partial class TestToolReflection
    {
        [UnityTest]
        public IEnumerator MethodCall_UnityEditor_EditorUserBuildSettings_get_activeBuildTarget()
        {
            var classType = typeof(UnityEditor.EditorUserBuildSettings);
            var name = "get_" + nameof(UnityEditor.EditorUserBuildSettings.activeBuildTarget);
            var methodInfo = classType.GetMethod(name);

            ResultValidation(new Tool_Reflection().MethodCall(
                filter: new MethodPointerRef(methodInfo)));

            yield return null;
        }
        [UnityTest]
        public IEnumerator MethodCall_UnityEditor_Build_NamedBuildTarget_get_TargetName()
        {
            var classType = typeof(UnityEditor.Build.NamedBuildTarget);
            var name = "get_" + nameof(UnityEditor.Build.NamedBuildTarget.TargetName);
            var methodInfo = classType.GetMethod(name);

            var obj = new UnityEditor.Build.NamedBuildTarget();
            var serializedObj = Reflector.Instance.Serialize(obj);

            ResultValidation(new Tool_Reflection().MethodCall(
                filter: new MethodPointerRef(methodInfo),
                targetObject: serializedObj));

            yield return null;
        }
        [UnityTest]
        public IEnumerator MethodCall_UnityEngine_Application_get_platform()
        {
            var classType = typeof(UnityEngine.Application);
            var name = "get_" + nameof(UnityEngine.Application.platform);
            var methodInfo = classType.GetMethod(name);
            var methodPointerRef = new MethodPointerRef(methodInfo);

            UnityEngine.Debug.Log($"Input: {methodPointerRef}\n");

            ResultValidation(new Tool_Reflection().MethodCall(
                filter: methodPointerRef));

            ResultValidation(new Tool_Reflection().MethodCall(
                filter: methodPointerRef,
                executeInMainThread: true));

            ResultValidation(new Tool_Reflection().MethodCall(
                filter: methodPointerRef,
                executeInMainThread: true,
                methodNameMatchLevel: 6));

            yield return null;
        }
    }
}