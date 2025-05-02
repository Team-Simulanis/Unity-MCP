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
            "Menu_ListSubmenus",
            Title = "List Unity Menu Submenus",
            Description = "List available submenus under a path even if direct items aren't found."
        )]
        public string ListSubmenus(
            [Description("Parent menu path. Empty for top-level menus.")]
            string parentPath = "")
        {
            Debug.Log($"[MCP] Menu.ListSubmenus called with parentPath: {parentPath}");
            
            return MainThread.Run(() =>
            {
                try
                {
                    // First try the regular menu items
                    var menuItems = MenuItemService.GetMenuItems(parentPath);
                    
                    // Then get all menu items to find possible submenus
                    var allMenuItems = MenuItemService.GetAllMenuItemsArray();
                    var possibleSubmenus = new HashSet<string>();
                    
                    // Build a full path prefix to search for
                    string prefix = string.IsNullOrEmpty(parentPath) ? "" : parentPath + "/";
                    
                    foreach (var item in allMenuItems)
                    {
                        if (string.IsNullOrEmpty(parentPath) || item.MenuPath.StartsWith(prefix))
                        {
                            string relevantPath = string.IsNullOrEmpty(parentPath) 
                                ? item.MenuPath 
                                : item.MenuPath.Substring(prefix.Length);
                                
                            int slashIndex = relevantPath.IndexOf('/');
                            
                            if (slashIndex >= 0)
                            {
                                string submenu = relevantPath.Substring(0, slashIndex);
                                
                                // For top-level, add the submenu as is
                                if (string.IsNullOrEmpty(parentPath))
                                {
                                    possibleSubmenus.Add(submenu);
                                }
                                // For other levels, add the full path to the submenu
                                else
                                {
                                    possibleSubmenus.Add($"{prefix}{submenu}");
                                }
                            }
                        }
                    }
                    
                    // Build a formatted string response
                    StringBuilder result = new StringBuilder();
                    
                    result.AppendLine($"# Menu Structure for '{(string.IsNullOrEmpty(parentPath) ? "Top Level" : parentPath)}'");
                    result.AppendLine();
                    
                    // Direct menu items
                    if (menuItems.Length > 0)
                    {
                        result.AppendLine($"## Direct Menu Items ({menuItems.Length})");
                        result.AppendLine();
                        
                        foreach (var item in menuItems.OrderBy(i => i.MenuPath))
                        {
                            result.AppendLine($"* `{item.MenuPath}` - {item.DisplayName}");
                            result.AppendLine($"  * To execute: `Menu_ExecuteItem(\"{item.MenuPath}\")`");
                            result.AppendLine();
                        }
                    }
                    
                    // Submenus
                    if (possibleSubmenus.Count > 0)
                    {
                        result.AppendLine($"## Submenus ({possibleSubmenus.Count})");
                        result.AppendLine();
                        
                        foreach (var submenu in possibleSubmenus.OrderBy(s => s))
                        {
                            result.AppendLine($"* `{submenu}`");
                            result.AppendLine($"  * To list items: `Menu_ListItems(\"{submenu}\")`");
                            result.AppendLine($"  * To explore further: `Menu_ListSubmenus(\"{submenu}\")`");
                            result.AppendLine();
                        }
                    }
                    
                    if (menuItems.Length == 0 && possibleSubmenus.Count == 0)
                    {
                        result.AppendLine("No menu items or submenus found for this path.");
                    }
                    
                    return result.ToString();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error listing submenus: {ex.Message}\n{ex.StackTrace}");
                    return $"Error listing submenus: {ex.Message}";
                }
            });
        }
    }
} 