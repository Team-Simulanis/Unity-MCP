using System.Collections;
using com.IvanMurzak.Unity.MCP.Common.Data.Utils;
using com.IvanMurzak.Unity.MCP.Editor.API;
using com.IvanMurzak.Unity.MCP.Utils;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace com.IvanMurzak.Unity.MCP.Editor.Tests
{
    public partial class TestToolGameObject
    {
        [UnityTest]
        public IEnumerator ModifyComponent_Vector3()
        {
            var child = new GameObject(GO_ParentName).AddChild(GO_Child1Name);
            var newPosition = new Vector3(1, 2, 3);

            var data = SerializedMember.FromValue(name: nameof(child.transform),
                    type: typeof(Transform),
                    value: new InstanceID(child.transform.GetInstanceID())
                )
                .AddProperty(SerializedMember.FromValue(name: nameof(child.transform.position),
                    value: newPosition));

            var result = new Tool_GameObject().ModifyComponent(data, 
                gameObjectRef: new Common.Data.Unity.GameObjectRef
                {
                    instanceID = child.GetInstanceID()
                });
            ResultValidation(result);

            Assert.IsTrue(result.Contains(GO_Child1Name), $"{GO_Child1Name} should be found in the path");
            Assert.AreEqual(child.transform.position, newPosition, "Position should be changed");
            Assert.AreEqual(child.transform.GetInstanceID(), data.GetInstanceID(), "InstanceID should be the same");
            yield return null;
        }
        [UnityTest]
        public IEnumerator ModifyComponent_Material()
        {
            var sharedMaterial = new Material(Shader.Find("Standard"));

            var go = new GameObject(GO_ParentName);
            var component = go.AddComponent<MeshRenderer>();

            var data = SerializedMember.FromValue(name: "",
                    type: typeof(MeshRenderer),
                    value: new InstanceID(component.GetInstanceID())
                )
                .AddProperty(SerializedMember.FromValue(name: nameof(component.sharedMaterial),
                    type: typeof(Material),
                    value: new InstanceID(sharedMaterial.GetInstanceID())));

            var result = new Tool_GameObject().ModifyComponent(data, 
                gameObjectRef: new Common.Data.Unity.GameObjectRef
                {
                    instanceID = go.GetInstanceID()
                });
            ResultValidation(result);

            Assert.IsTrue(result.Contains(GO_ParentName), $"{GO_ParentName} should be found in the path");
            Assert.AreEqual(component.sharedMaterial.GetInstanceID(), sharedMaterial.GetInstanceID(), "Materials InstanceIDs should be the same.");
            yield return null;
        }
    }
}