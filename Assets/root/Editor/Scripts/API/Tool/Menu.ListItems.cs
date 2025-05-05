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
                    
                    // Build a formatted string response
                    StringBuilder result = new StringBuilder();
                    
                    // Title with horizontal line
                    string title = string.IsNullOrEmpty(parentPath) 
                        ? "Top-Level Menu Categories" 
                        : $"Menu Items for '{parentPath}'";
                    
                    result.AppendLine($"# {title}");
                    result.AppendLine($"Found {menuItems.Length} items");
                    result.AppendLine(new string('-', 50));
                    result.AppendLine();

                    // Create a nicely formatted table
                    if (menuItems.Length > 0)
                    {
                        // For top-level menus, show different information
                        if (string.IsNullOrEmpty(parentPath))
                        {
                            // Group by Category to remove duplicates
                            var uniqueCategories = menuItems
                                .OrderBy(m => m.Category)
                                .GroupBy(m => m.Category)
                                .Select(g => g.First());
                                
                            result.AppendLine("| Category | Execute Command |");
                            result.AppendLine("|----------|----------------|");
                            
                            foreach (var item in uniqueCategories)
                            {
                                result.AppendLine($"| **{item.Category}** | `Menu_ListItems(\"{item.Category}\")` |");
                            }
                        }
                        else
                        {
                            // For submenu items
                            result.AppendLine("| Menu Path | Command to Execute |");
                            result.AppendLine("|-----------|---------------------|");
                            
                            foreach (var item in menuItems.OrderBy(m => m.MenuPath))
                            {
                                // Extract the display name (remove parent path)
                                string displayName = item.MenuPath;
                                if (!string.IsNullOrEmpty(parentPath) && item.MenuPath.StartsWith(parentPath + "/"))
                                {
                                    displayName = item.MenuPath.Substring(parentPath.Length + 1);
                                }
                                
                                result.AppendLine($"| **{displayName}** | `Menu_ExecuteItem(\"{item.MenuPath}\")` |");
                            }
                        }
                    }
                    else
                    {
                        result.AppendLine("No menu items found for this path.");
                    }
                    
                    // Add helpful tips
                    result.AppendLine();
                    result.AppendLine("## Tips");
                    result.AppendLine("- To execute a menu item: `Menu_ExecuteItem(\"full/path/to/item\")`");
                    result.AppendLine("- To explore a category: `Menu_ListItems(\"category/path\")`");
                    result.AppendLine("- Common categories: File, Edit, Window, GameObject, Tools");
                    
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