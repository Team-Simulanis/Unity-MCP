using System;
using System.Linq;
using System.Text;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Common.Data;
using com.IvanMurzak.Unity.MCP.Unity;
using com.IvanMurzak.Unity.MCP.Utils;
using System.ComponentModel;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Menu
    {
        [McpPluginTool
        (
            "Menu_ListItems",
            Title = "List Unity Menu Items",
            Description = "List available Unity menu items. Provide a parent path to see child items."
        )]
        public string ListItems(
            [Description("Parent menu path. Empty for top-level menus.")]
            string parentPath = "")
        {
            Debug.Log($"[MCP] Menu.ListItems called with parentPath: {parentPath}");
            
            return MainThread.Run(() =>
            {
                try
                {
                    Debug.Log($"[MCP] Fetching menu items for path: {parentPath}");
                    var menuItems = MenuItemService.GetMenuItems(parentPath);
                    Debug.Log($"[MCP] Retrieved {menuItems.Length} menu items");
                    
                    // Log the menu items data for debugging
                    foreach (var item in menuItems)
                    {
                        Debug.Log($"Menu item: {item.MenuPath}, {item.DisplayName}, {item.Category}, {item.IsEnabled}");
                    }
                    
                    // Build a formatted string response
                    StringBuilder result = new StringBuilder();
                    result.AppendLine($"Found {menuItems.Length} menu items for path '{parentPath}':");
                    result.AppendLine();
                    
                    foreach (var item in menuItems)
                    {
                        result.AppendLine($"• {item.MenuPath}");
                        result.AppendLine($"  Display: {item.DisplayName}");
                        result.AppendLine($"  Category: {item.Category}");
                        result.AppendLine($"  Enabled: {item.IsEnabled}");
                        result.AppendLine();
                    }
                    
                    return result.ToString();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error listing menus: {ex.Message}\n{ex.StackTrace}");
                    return $"Error listing menus: {ex.Message}";
                }
            });
        }
    }
} 