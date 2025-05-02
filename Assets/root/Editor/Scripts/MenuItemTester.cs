using UnityEngine;
using UnityEditor;
using com.IvanMurzak.Unity.MCP.Unity;
using com.IvanMurzak.Unity.MCP.Common.Data;

namespace com.IvanMurzak.Unity.MCP.Editor
{
    public static class MenuItemTester
    {
        [MenuItem("Tools/AI Connector (Unity-MCP)/Test/List All Menu Items")]
        public static void TestListAllMenuItems()
        {
            var items = MenuItemService.GetAllMenuItemsArray();
            Debug.Log($"Found {items.Length} total menu items");
            
            // Output the first 10 items as a sample
            for (int i = 0; i < Mathf.Min(items.Length, 10); i++)
            {
                Debug.Log($"Menu {i}: {items[i].MenuPath} (Category: {items[i].Category})");
            }
        }
        
        [MenuItem("Tools/AI Connector (Unity-MCP)/Test/List Top-Level Categories")]
        public static void TestListTopLevelCategories()
        {
            var items = MenuItemService.GetMenuItems();
            Debug.Log($"Found {items.Length} top-level categories");
            
            foreach (var item in items)
            {
                Debug.Log($"Category: {item.Category}");
            }
        }
        
        [MenuItem("Tools/AI Connector (Unity-MCP)/Test/List File Menu Items")]
        public static void TestListFileMenuItems()
        {
            var items = MenuItemService.GetMenuItems("File");
            Debug.Log($"Found {items.Length} items in File menu");
            
            foreach (var item in items)
            {
                Debug.Log($"File Menu Item: {item.MenuPath} (Display: {item.DisplayName})");
            }
        }
        
        [MenuItem("Tools/AI Connector (Unity-MCP)/Test/Execute Save Menu Item")]
        public static void TestExecuteSaveMenuItem()
        {
            var result = MenuItemService.ExecuteMenuItem("File/Save");
            Debug.Log($"Executed File/Save: Success={result.Success}, Message={result.Message}");
        }
    }
} 