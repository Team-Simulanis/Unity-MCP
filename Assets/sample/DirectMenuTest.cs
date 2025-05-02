using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

#if UNITY_EDITOR
public class DirectMenuTest
{
    [MenuItem("Tools/Test/Print Menu Items")]
    public static void PrintMenuItems()
    {
        try
        {
            // Try to get menu items through reflection
            var menuItemsType = typeof(Menu);
            var getMenuItemsMethod = menuItemsType.GetMethod("GetMenuItems", 
                BindingFlags.Static | BindingFlags.NonPublic);
                
            if (getMenuItemsMethod != null)
            {
                var menuItems = getMenuItemsMethod.Invoke(null, null) as Array;
                
                if (menuItems != null)
                {
                    Debug.Log($"Found {menuItems.Length} menu items");
                    
                    // Print a sample of items (first 10)
                    for (int i = 0; i < Math.Min(10, menuItems.Length); i++)
                    {
                        var item = menuItems.GetValue(i);
                        var itemType = item.GetType();
                        
                        // Get the menu path using reflection
                        var menuPathField = itemType.GetField("menuPath");
                        var menuPath = menuPathField?.GetValue(item) as string;
                        
                        Debug.Log($"Menu item {i}: {menuPath}");
                    }
                }
                else
                {
                    Debug.LogError("Menu.GetMenuItems returned null");
                }
            }
            else
            {
                Debug.LogError("Method Menu.GetMenuItems not found");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error accessing menu items: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
#endif