using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Common.Data;
using com.IvanMurzak.Unity.MCP.Unity;
using com.IvanMurzak.Unity.MCP.Utils;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Menu
    {
        [McpPluginTool(
            "Menu_GetMenuReference",
            Title = "Get Menu Item Reference",
            Description = "Returns a detailed reference for all menu items under a specific path"
        )]
        public string GetMenuReference(
            [Description("Root menu path to document (e.g., 'Tools/McpTool'). Leave empty for all top-level menus.")]
            string menuPath = ""
        )
        {
            Debug.Log($"[MCP] Menu.GetMenuReference called with menuPath: {menuPath}");
            
            return MainThread.Run(() =>
            {
                try
                {
                    StringBuilder result = new StringBuilder();
                    
                    if (string.IsNullOrEmpty(menuPath))
                    {
                        result.AppendLine("# All Unity Menu Categories");
                        result.AppendLine();
                        var topMenus = MenuItemService.GetMenuItems();
                        foreach (var menu in topMenus)
                        {
                            result.AppendLine($"* `{menu.MenuPath}`");
                        }
                        
                        result.AppendLine();
                        result.AppendLine("Use `Menu_GetMenuReference(\"category\")` to see items in a specific category.");
                        result.AppendLine("⚠️ Note: Some categories might not show direct items but still have submenus.");
                        result.AppendLine("For example, try `Menu_ListItems(\"Tools/AI Connector (Unity-MCP)\")` to see specific submenus.");
                        return result.ToString();
                    }
                    
                    result.AppendLine($"# Menu Reference for '{menuPath}'");
                    result.AppendLine();
                    
                    // Get direct children of this menu path
                    var menuItems = MenuItemService.GetMenuItems(menuPath);
                    
                    // Check for known submenus even if there are no direct items
                    if (menuItems.Length == 0)
                    {
                        result.AppendLine($"No direct menu items found under '{menuPath}'.");
                        result.AppendLine();
                        result.AppendLine("However, there might be submenus. Try these common paths:");
                        result.AppendLine();
                        
                        // Suggest common submenus based on the path
                        if (menuPath.Equals("Tools", StringComparison.OrdinalIgnoreCase))
                        {
                            result.AppendLine("* `Tools/AI Connector (Unity-MCP)` - AI Connector submenu");
                            result.AppendLine("* `Tools/Test` - Test submenu");
                            result.AppendLine();
                            result.AppendLine("To view items in these submenus, use:");
                            result.AppendLine("`Menu_ListItems(\"Tools/AI Connector (Unity-MCP)\")`");
                        }
                        else if (menuPath.Equals("Window", StringComparison.OrdinalIgnoreCase))
                        {
                            result.AppendLine("* `Window/General` - General windows submenu");
                            result.AppendLine("* `Window/Analysis` - Analysis tools submenu");
                            result.AppendLine();
                            result.AppendLine("To view items in these submenus, use:");
                            result.AppendLine("`Menu_ListItems(\"Window/General\")`");
                        }
                        
                        // Try to find known submenus by checking all menu items
                        var allMenuItems = MenuItemService.GetAllMenuItemsArray();
                        var possibleSubmenus = new HashSet<string>();
                        
                        foreach (var item in allMenuItems)
                        {
                            if (item.MenuPath.StartsWith(menuPath + "/"))
                            {
                                string remaining = item.MenuPath.Substring(menuPath.Length + 1);
                                int slashIndex = remaining.IndexOf('/');
                                
                                if (slashIndex >= 0)
                                {
                                    string submenu = remaining.Substring(0, slashIndex);
                                    possibleSubmenus.Add(submenu);
                                }
                            }
                        }
                        
                        if (possibleSubmenus.Count > 0)
                        {
                            result.AppendLine("## Detected Submenus");
                            result.AppendLine();
                            
                            foreach (var submenu in possibleSubmenus.OrderBy(s => s))
                            {
                                string fullPath = $"{menuPath}/{submenu}";
                                result.AppendLine($"* `{fullPath}`");
                                result.AppendLine($"  * To list items: `Menu_ListItems(\"{fullPath}\")`");
                                result.AppendLine();
                            }
                        }
                        
                        return result.ToString();
                    }
                    
                    // Group items by their immediate submenu
                    var groupedItems = new Dictionary<string, List<ResponseMenuItem>>();
                    
                    foreach (var item in menuItems)
                    {
                        string itemPath = item.MenuPath;
                        
                        // Skip the parent path
                        if (itemPath.Length <= menuPath.Length)
                            continue;
                        
                        string relativePath = itemPath.Substring(menuPath.Length + 1); // +1 for the '/'
                        
                        // Check if this is a direct item or a submenu
                        int slashIndex = relativePath.IndexOf('/');
                        if (slashIndex >= 0)
                        {
                            // This is a submenu
                            string submenuName = relativePath.Substring(0, slashIndex);
                            string key = $"{menuPath}/{submenuName}";
                            
                            if (!groupedItems.ContainsKey(key))
                                groupedItems[key] = new List<ResponseMenuItem>();
                            
                            groupedItems[key].Add(item);
                        }
                        else
                        {
                            // This is a direct item
                            if (!groupedItems.ContainsKey(menuPath))
                                groupedItems[menuPath] = new List<ResponseMenuItem>();
                            
                            groupedItems[menuPath].Add(item);
                        }
                    }
                    
                    // Now output each group
                    foreach (var group in groupedItems.OrderBy(g => g.Key))
                    {
                        // For the main menu, don't add a section header
                        if (group.Key != menuPath)
                        {
                            string submenuName = group.Key.Substring(menuPath.Length + 1);
                            result.AppendLine($"## {submenuName}");
                            result.AppendLine();
                            result.AppendLine($"Submenu path: `{group.Key}`");
                            result.AppendLine();
                            result.AppendLine($"To list all items in this submenu: `Menu_ListItems(\"{group.Key}\")`");
                            result.AppendLine();
                            
                            // Check if there are more submenus
                            if (group.Value.Any(i => i.MenuPath.IndexOf('/', group.Key.Length + 1) >= 0))
                            {
                                result.AppendLine($"This submenu contains nested submenus. Use `Menu_GetMenuReference(\"{group.Key}\")` to explore further.");
                                result.AppendLine();
                            }
                        }
                        else
                        {
                            result.AppendLine("## Direct Menu Items");
                            result.AppendLine();
                        }
                        
                        // List the items in this group
                        var directItems = group.Value
                            .Where(i => i.MenuPath.StartsWith(group.Key + "/") && i.MenuPath.IndexOf('/', group.Key.Length + 1) < 0)
                            .OrderBy(i => i.MenuPath);
                        
                        foreach (var item in directItems)
                        {
                            result.AppendLine($"* `{item.MenuPath}` - {item.DisplayName}");
                            result.AppendLine($"  * To execute: `Menu_ExecuteItem(\"{item.MenuPath}\")`");
                            result.AppendLine();
                        }
                    }
                    
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