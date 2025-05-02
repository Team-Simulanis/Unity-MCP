using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Unity;
using com.IvanMurzak.Unity.MCP.Utils;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Menu
    {
        [McpPluginTool(
            "Menu_GetReference",
            Title = "Get Menu Reference",
            Description = "Returns a reference guide for commonly used Unity menu items"
        )]
        public string GetReference(
            [Description("Category to filter by (e.g., 'File', 'Tools', 'Window'). Leave empty for all categories.")]
            string category = ""
        )
        {
            Debug.Log($"[MCP] Menu.GetReference called with category: {category}");
            
            return MainThread.Run(() =>
            {
                try
                {
                    StringBuilder result = new StringBuilder();
                    result.AppendLine("# Unity Menu Reference Guide");
                    result.AppendLine();
                    
                    // First, add the common predefined menu items with descriptions
                    result.AppendLine("## Common Menu Items");
                    result.AppendLine();
                    
                    foreach (var kvp in MenuPathDescriptions)
                    {
                        if (string.IsNullOrEmpty(category) || kvp.Key.StartsWith(category + "/", StringComparison.OrdinalIgnoreCase))
                        {
                            result.AppendLine($"* `{kvp.Key}` - {kvp.Value}");
                        }
                    }
                    
                    result.AppendLine();
                    result.AppendLine("## Top-Level Menus");
                    result.AppendLine();
                    
                    // List top-level menus
                    var topMenus = MenuItemService.GetMenuItems();
                    foreach (var item in topMenus)
                    {
                        result.AppendLine($"* `{item.MenuPath}` - Top-level menu category");
                    }
                    
                    result.AppendLine();
                    result.AppendLine("## How to Use Menus with MCP");
                    result.AppendLine();
                    result.AppendLine("1. To list menu items in a category: `Menu_ListItems(\"category\")`");
                    result.AppendLine("   Example: `Menu_ListItems(\"File\")`");
                    result.AppendLine();
                    result.AppendLine("2. To list submenu items: `Menu_ListItems(\"category/submenu\")`");
                    result.AppendLine("   Example: `Menu_ListItems(\"Tools/AI Connector (Unity-MCP)\")`");
                    result.AppendLine();
                    result.AppendLine("3. To execute a menu item: `Menu_ExecuteItem(\"full/path/to/item\")`");
                    result.AppendLine("   Example: `Menu_ExecuteItem(\"File/New Scene %n\")`");
                    
                    // Add reference to our new submenu discovery tool
                    result.AppendLine();
                    result.AppendLine("4. To discover submenus: `Menu_ListSubmenus(\"category\")`");
                    result.AppendLine("   Example: `Menu_ListSubmenus(\"Tools\")`");
                    
                    return result.ToString();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error generating menu reference: {ex.Message}\n{ex.StackTrace}");
                    return $"Error generating menu reference: {ex.Message}";
                }
            });
        }
    }
} 