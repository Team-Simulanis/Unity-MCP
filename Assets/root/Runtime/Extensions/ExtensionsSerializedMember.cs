#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using com.IvanMurzak.Unity.MCP.Common.Data.Utils;

namespace com.IvanMurzak.Unity.MCP.Utils
{
    public static class ExtensionsSerializedMember
    {
        public static int GetInstanceID(this SerializedMember member)
            => member.GetValue<InstanceID>()?.instanceID
                ?? member.GetField("instanceID")?.GetValue<int>()
                ?? 0;
        public static bool GetIndexFromName(this SerializedMember member, out int index)
        {
            if (HasIndexName(member) == false)
            {
                index = -1;
                return false;
            }
            return int.TryParse(member.name.Substring(1, member.name.Length - 2), out index);
        }
        public static bool HasIndexName(this SerializedMember member)
            => member.name?.Length >= 3 && member.name[0] == '[' && member.name[^1] == ']';
    }
}