#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.Reflection;
using com.IvanMurzak.Unity.MCP.Common.Data;

namespace com.IvanMurzak.Unity.MCP.Common.Utils
{
    public static class MenuItemHelper
    {
        public static ResponseMenuItem[] GetMenuItems()
        {
            #if UNITY_EDITOR
            // Use reflection to access the MenuItemService
            try
            {
                var type = Type.GetType("com.IvanMurzak.Unity.MCP.Unity.MenuItemService, Assembly-CSharp");
                if (type == null)
                {
                    return new ResponseMenuItem[0];
                }
                
                var method = type.GetMethod("GetMenuItems", BindingFlags.Public | BindingFlags.Static);
                if (method == null)
                {
                    return new ResponseMenuItem[0];
                }
                
                return (ResponseMenuItem[])method.Invoke(null, null);
            }
            catch (Exception ex)
            {
               // UnityEngine.Debug.LogError($"Error getting menu items: {ex.Message}");
                return new ResponseMenuItem[0];
            }
            #else
            return new ResponseMenuItem[0];
            #endif
        }
        
        public static ResponseExecuteMenuItem ExecuteMenuItem(string menuPath)
        {
            #if UNITY_EDITOR
            // Use reflection to access the MenuItemService
            try
            {
                var type = Type.GetType("com.IvanMurzak.Unity.MCP.Unity.MenuItemService, Assembly-CSharp");
                if (type == null)
                {
                    return new ResponseExecuteMenuItem(menuPath, false, "MenuItemService type not found");
                }
                
                var method = type.GetMethod("ExecuteMenuItem", BindingFlags.Public | BindingFlags.Static);
                if (method == null)
                {
                    return new ResponseExecuteMenuItem(menuPath, false, "ExecuteMenuItem method not found");
                }
                
                return (ResponseExecuteMenuItem)method.Invoke(null, new object[] { menuPath });
            }
            catch (Exception ex)
            {
              //  UnityEngine.Debug.LogError($"Error executing menu item: {ex.Message}");
                return new ResponseExecuteMenuItem(menuPath, false, $"Error: {ex.Message}");
            }
            #else
            return new ResponseExecuteMenuItem(menuPath, false, "Menu item execution is only available in the Unity Editor");
            #endif
        }
    }
} 