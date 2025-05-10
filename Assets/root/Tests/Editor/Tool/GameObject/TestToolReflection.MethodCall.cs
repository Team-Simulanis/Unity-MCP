using System.Collections;
using com.IvanMurzak.Unity.MCP.Common.Data.Unity;
using com.IvanMurzak.Unity.MCP.Editor.API;
using UnityEngine.TestTools;

namespace com.IvanMurzak.Unity.MCP.Editor.Tests
{
    public partial class TestToolReflection
    {
        [UnityTest]
        public IEnumerator MethodCall_UnityEditor_EditorUserBuildSettings_activeBuildTarget()
        {
            var classType = typeof(UnityEditor.EditorUserBuildSettings);
            var name = "get_" + nameof(UnityEditor.EditorUserBuildSettings.activeBuildTarget);
            var methodInfo = classType.GetMethod(name);

            var result = new Tool_Reflection().MethodCall(
                filter: new MethodPointerRef(methodInfo));

            ResultValidation(result);
            yield return null;
        }
        [UnityTest]
        public IEnumerator MethodCall_UnityEditor_Build_NamedBuildTarget_TargetName()
        {
            var classType = typeof(UnityEditor.Build.NamedBuildTarget);
            var name = "get_" + nameof(UnityEditor.Build.NamedBuildTarget.TargetName);
            var methodInfo = classType.GetMethod(name);

            var result = new Tool_Reflection().MethodCall(
                filter: new MethodPointerRef(methodInfo));

            ResultValidation(result);
            yield return null;
        }
    }
}