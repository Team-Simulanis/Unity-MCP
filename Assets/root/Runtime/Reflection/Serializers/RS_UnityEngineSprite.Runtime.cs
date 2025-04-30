#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#if !UNITY_EDITOR
using System.Linq;
using System.Reflection;
using System.Text;
using com.IvanMurzak.Unity.MCP.Common.Data.Utils;
using UnityEditor;

namespace com.IvanMurzak.Unity.MCP.Utils
{
    public partial class RS_UnityEngineSprite : RS_UnityEngineObject<UnityEngine.Sprite>
    {
        public override StringBuilder Populate(ref object obj, SerializedMember data, int depth = 0, StringBuilder stringBuilder = null, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            return stringBuilder?.AppendLine($"[Error] Operation is not supported in runtime.");
        }
    }
}
#endif