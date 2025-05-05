using UnityEngine;
using UnityEditor;
using com.IvanMurzak.Unity.MCP.Editor.API;
using com.IvanMurzak.Unity.MCP.Common;
using System.Reflection;

namespace com.IvanMurzak.Unity.MCP.Editor
{
    [InitializeOnLoad]
    public static class ToolRegister
    {
        static ToolRegister()
        {
            Debug.Log("[MCP] Registering custom tools...");
            
            // Ensure our assembly is loaded and discovered
            RegisterCustomTools();
        }
        
        [MenuItem("Tools/AI Connector (Unity-MCP)/Register Custom Tools", priority = 2000)]
        public static void RegisterCustomTools()
        {
            Debug.Log("[MCP] Explicitly registering custom tools...");
            
            // Get the current assembly
            var assembly = Assembly.GetExecutingAssembly();
            
            // Log the types with McpPluginToolType attribute
            var toolTypes = assembly.GetTypes();
            foreach (var type in toolTypes)
            {
                var attr = type.GetCustomAttribute<McpPluginToolTypeAttribute>();
                if (attr != null)
                {
                    Debug.Log($"[MCP] Found tool type: {type.FullName} with path: {attr.Path}");
                    
                    // Log the methods with McpPluginTool attribute
                    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
                    {
                        var methodAttr = method.GetCustomAttribute<McpPluginToolAttribute>();
                        if (methodAttr != null)
                        {
                            Debug.Log($"[MCP] Found tool method: {method.Name} with name: {methodAttr.Name}");
                        }
                    }
                }
            }
            
            // Explicitly ensure the Tool_Menu class is loaded
            var menuToolType = typeof(Tool_Menu);
            Debug.Log($"[MCP] Ensured Tool_Menu type is loaded: {menuToolType.FullName}");
        }
    }
} 