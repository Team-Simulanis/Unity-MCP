using UnityEngine;
using UnityEditor;
using com.IvanMurzak.Unity.MCP.Unity;
using System.Linq;
using System.Text;
using com.IvanMurzak.Unity.MCP.Common;


namespace com.IvanMurzak.Unity.MCP.Editor
{
    public static class MenuTest
    {
        [MenuItem("Tools/AI Connector (Unity-MCP)/Test/List Top-Level Menus", priority = 1500)]
        public static void TestListTopLevelMenus()
        {
            var menuItems = MenuItemService.GetMenuItems();
            
            // Output structured data as JSON
            var json = JsonUtils.Serialize(menuItems);
            Debug.Log($"Found {menuItems.Length} top-level menus:");
            
            // Pretty-print the top-level menus
            var sb = new StringBuilder();
            sb.AppendLine("Top-level menus:");
            sb.AppendLine("---------------");
            foreach (var item in menuItems)
            {
                sb.AppendLine($"• {item.MenuPath}");
            }
            Debug.Log(sb.ToString());
            
            // Save to a temporary file for easier inspection
            var tempPath = System.IO.Path.Combine(Application.dataPath, "../Temp/top_level_menus.json");
            System.IO.File.WriteAllText(tempPath, json);
            Debug.Log($"Saved top-level menus to: {tempPath}");
        }
        
        [MenuItem("Tools/AI Connector (Unity-MCP)/Test/List File Menu Items", priority = 1501)]
        public static void TestListFileMenuItems()
        {
            var menuItems = MenuItemService.GetMenuItems("File");
            
            // Output structured data as JSON
            var json = JsonUtils.Serialize(menuItems);
            Debug.Log($"Found {menuItems.Length} items in the File menu:");
            
            // Pretty-print in a tree format
            var sb = new StringBuilder();
            sb.AppendLine("File menu items:");
            sb.AppendLine("--------------");
            foreach (var item in menuItems)
            {
                sb.AppendLine($"• {item.DisplayName} ({item.MenuPath})");
            }
            Debug.Log(sb.ToString());
            
            // Save to a temporary file for easier inspection
            var tempPath = System.IO.Path.Combine(Application.dataPath, "../Temp/file_menu_items.json");
            System.IO.File.WriteAllText(tempPath, json);
            Debug.Log($"Saved File menu items to: {tempPath}");
        }
        
        [MenuItem("Tools/AI Connector (Unity-MCP)/Test/List GameObject Menu Items", priority = 1502)]
        public static void TestListGameObjectMenuItems()
        {
            var menuItems = MenuItemService.GetMenuItems("GameObject");
            
            // Output structured data as JSON
            var json = JsonUtils.Serialize(menuItems);
            Debug.Log($"Found {menuItems.Length} items in the GameObject menu:");
            
            // Pretty-print in a tree format
            var sb = new StringBuilder();
            sb.AppendLine("GameObject menu items:");
            sb.AppendLine("---------------------");
            foreach (var item in menuItems)
            {
                sb.AppendLine($"• {item.DisplayName} ({item.MenuPath})");
            }
            Debug.Log(sb.ToString());
            
            // Save to a temporary file for easier inspection
            var tempPath = System.IO.Path.Combine(Application.dataPath, "../Temp/gameobject_menu_items.json");
            System.IO.File.WriteAllText(tempPath, json);
            Debug.Log($"Saved GameObject menu items to: {tempPath}");
        }
        
        [MenuItem("Tools/AI Connector (Unity-MCP)/Test/Execute Menu Item", priority = 1503)]
        public static void TestExecuteMenuItem()
        {
            var result = MenuItemService.ExecuteMenuItem("Window/AI Connector (Unity-MCP)");
            Debug.Log($"Execute result: {result.Success}, Message: {result.Message}");
        }
        
        [MenuItem("Tools/AI Connector (Unity-MCP)/Test/List All Menu Items", priority = 1499)]
        public static void TestListAllMenuItems()
        {
            var menuItems = MenuItemService.GetAllMenuItemsArray();
            
            // Output structured data as JSON
            var json = JsonUtils.Serialize(menuItems);
            Debug.Log($"Found {menuItems.Length} total menu items");
            
            // Save to a temporary file for easier inspection
            var tempPath = System.IO.Path.Combine(Application.dataPath, "../Temp/all_menu_items.json");
            System.IO.File.WriteAllText(tempPath, json);
            Debug.Log($"Saved all menu items to: {tempPath}");
            
            // Log categorized count
            var categories = menuItems.GroupBy(m => m.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count);
                
            var sb = new StringBuilder();
            sb.AppendLine("Menu categories:");
            sb.AppendLine("---------------");
            foreach (var category in categories)
            {
                sb.AppendLine($"• {category.Category}: {category.Count} items");
            }
            Debug.Log(sb.ToString());
        }

        [MenuItem("Tools/AI Connector (Unity-MCP)/Test/Execute - Game Window", priority = 1510)]
        public static void TestExecuteGameWindow()
        {
            var result = MenuItemService.ExecuteMenuItem("Window/General/Game");
            Debug.Log($"Game window execution result: {result.Success}, Message: {result.Message}");
        }

        [MenuItem("Tools/AI Connector (Unity-MCP)/Test/Execute - Console Window", priority = 1511)]
        public static void TestExecuteConsoleWindow()
        {
            var result = MenuItemService.ExecuteMenuItem("Window/General/Console");
            Debug.Log($"Console window execution result: {result.Success}, Message: {result.Message}");
        }

        [MenuItem("Tools/AI Connector (Unity-MCP)/Test/Execute - Scene View Focus", priority = 1512)]
        public static void TestExecuteSceneFocus()
        {
            var result = MenuItemService.ExecuteMenuItem("Window/General/Scene");
            Debug.Log($"Scene view execution result: {result.Success}, Message: {result.Message}");
        }

        [MenuItem("Tools/AI Connector (Unity-MCP)/Test/Execute - Project Window", priority = 1513)]
        public static void TestExecuteProjectWindow()
        {
            var result = MenuItemService.ExecuteMenuItem("Window/General/Project");
            Debug.Log($"Project window execution result: {result.Success}, Message: {result.Message}");
        }
    }
} 