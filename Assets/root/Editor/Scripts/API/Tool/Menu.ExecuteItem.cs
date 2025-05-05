using System;
using System.ComponentModel;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Common.Data;
using com.IvanMurzak.Unity.MCP.Unity;
using com.IvanMurzak.Unity.MCP.Utils;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Menu
    {
        [McpPluginTool
        (
            "Menu_ExecuteItem",
            Title = "Execute a Unity Menu Item"
        )]
        [System.ComponentModel.Description("Execute a Unity menu item by specifying its menu path")]
        public string ExecuteItem
        (
            [System.ComponentModel.Description("Path to the menu item to execute")]
            string menuPath = "Window/AI Connector (Unity-MCP)"
        )
        {
            Debug.Log($"[MCP] Menu.ExecuteItem called with menuPath: {menuPath}");
            
            return MainThread.Run(() =>
            {
                try
                {
                    Debug.Log($"[MCP] Executing menu item: {menuPath}");
                    var result = MenuItemService.ExecuteMenuItem(menuPath);
                    
                    string response;
                    if (result.Success)
                    {
                        response = $"✅ Successfully executed menu item: {menuPath}";
                        Debug.Log($"[MCP] {response}");
                    }
                    else
                    {
                        response = $"❌ Failed to execute menu item: {menuPath}\nMessage: {result.Message}";
                        Debug.LogError($"[MCP] {response}");
                    }
                    
                    return response;
                }
                catch (Exception ex)
                {
                    string error = $"⚠️ Exception executing menu: {ex.Message}\n{ex.StackTrace}";
                    Debug.LogError($"[MCP] {error}");
                    return error;
                }
            });
        }
    }
} 