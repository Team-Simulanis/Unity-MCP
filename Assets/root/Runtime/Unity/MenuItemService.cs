using System;
using System.Collections.Generic;
using System.Linq;
using com.IvanMurzak.Unity.MCP.Common.Data;
using com.IvanMurzak.Unity.MCP.Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.Unity
{
#if UNITY_EDITOR
    public class MenuItemService
    {
        // Cache all menu items to avoid repeatedly fetching them
        private static List<ResponseMenuItem> _allMenuItems;
        
        public static ResponseMenuItem[] GetMenuItems(string parentPath = "")
        {
            // Initialize or use cached menu items
            if (_allMenuItems == null)
            {
                _allMenuItems = GetAllMenuItems();
                Debug.Log($"Cached {_allMenuItems.Count} menu items");
            }
            
            // If parent path is empty, return only top-level menus
            if (string.IsNullOrEmpty(parentPath))
            {
                var uniqueCategories = new HashSet<string>();
                var topLevelMenus = new List<ResponseMenuItem>();
                
                foreach (var item in _allMenuItems)
                {
                    string category = item.Category;
                    
                    // Only add each top-level category once
                    if (!string.IsNullOrEmpty(category) && !uniqueCategories.Contains(category))
                    {
                        uniqueCategories.Add(category);
                        topLevelMenus.Add(new ResponseMenuItem(category, category, true, category));
                    }
                }
                
                return topLevelMenus.OrderBy(m => m.Category).ToArray();
            }
            // Otherwise, return child items for the specified parent path
            else
            {
                var childMenus = new List<ResponseMenuItem>();
                
                foreach (var item in _allMenuItems)
                {
                    // Check if this item is a direct child of the parent path
                    if (IsDirectChild(item.MenuPath, parentPath))
                    {
                        childMenus.Add(item);
                    }
                }
                
                return childMenus.OrderBy(m => m.MenuPath).ToArray();
            }
        }
        
        // For debugging and direct access to all menu items
        public static ResponseMenuItem[] GetAllMenuItemsArray()
        {
            if (_allMenuItems == null)
            {
                _allMenuItems = GetAllMenuItems();
            }
            return _allMenuItems.ToArray();
        }
        
        // Checks if a menu path is a direct child of a parent path
        private static bool IsDirectChild(string menuPath, string parentPath)
        {
            if (string.IsNullOrEmpty(menuPath) || !menuPath.StartsWith(parentPath))
                return false;
                
            // Remove the parent path from the menu path
            string remainingPath = menuPath.Substring(parentPath.Length);
            
            // If the parent path doesn't end with a slash, the remaining path should start with a slash
            if (!parentPath.EndsWith("/"))
            {
                if (!remainingPath.StartsWith("/"))
                    return false;
                    
                remainingPath = remainingPath.Substring(1); // Remove the leading slash
            }
            
            // Check if there are any more slashes in the remaining path
            // If there are, it's a grandchild, not a direct child
            return !remainingPath.Contains('/');
        }
        
        // This method collects all menu items from Unity
        private static List<ResponseMenuItem> GetAllMenuItems()
        {
            var menuItems = new List<ResponseMenuItem>();
            
            try
            {
                // Try all available methods to get the most complete menu item list
                menuItems = CollectMenuItemsViaReflection();
                
                if (menuItems.Count == 0)
                {
                    Debug.LogWarning("Failed to get menu items via reflection, trying MenuItem attributes");
                    menuItems = FindMenuItemAttributesInAssemblies();
                }
                
                if (menuItems.Count == 0)
                {
                    Debug.LogWarning("Failed to get menu items via attributes, using fallback list");
                    menuItems = GetCommonMenuItems();
                }
                
                Debug.Log($"Total menu items collected: {menuItems.Count}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error collecting menu items: {ex.Message}");
                // Ensure we at least have some menu items
                menuItems = GetCommonMenuItems();
            }
            
            return menuItems;
        }
        
        // Collects menu items using reflection on Unity's internal Menu class
        private static List<ResponseMenuItem> CollectMenuItemsViaReflection()
        {
            var menuItems = new List<ResponseMenuItem>();
            
            try
            {
                // First try EditorGUIUtility.GetMainMenuPaths if available
                var editorAssembly = typeof(EditorWindow).Assembly;
                var editorGUIUtilityType = editorAssembly.GetType("UnityEditor.EditorGUIUtility");
                
                if (editorGUIUtilityType != null)
                {
                    var getMainMenuPathsMethod = editorGUIUtilityType.GetMethod("GetMainMenuPaths", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                        
                    if (getMainMenuPathsMethod != null)
                    {
                        var menuPaths = getMainMenuPathsMethod.Invoke(null, null) as string[];
                        
                        if (menuPaths != null && menuPaths.Length > 0)
                        {
                            Debug.Log($"Found {menuPaths.Length} menu paths via EditorGUIUtility");
                            foreach (var menuPath in menuPaths)
                            {
                                if (string.IsNullOrEmpty(menuPath) || menuPath.StartsWith("_"))
                                    continue;
                                    
                                AddMenuItemToList(menuItems, menuPath);
                            }
                        }
                    }
                }
                
                // If we still don't have menu items, try Menu.GetMenuItems
                if (menuItems.Count == 0)
                {
                    var menuItemsType = typeof(Menu);
                    var menuItemsMethod = menuItemsType.GetMethod("GetMenuItems", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                    
                    if (menuItemsMethod != null)
                    {
                        // Get parameters with proper default values
                        var parameters = menuItemsMethod.GetParameters();
                        object[] args = new object[parameters.Length];
                        
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            args[i] = parameters[i].ParameterType.IsValueType ? 
                                Activator.CreateInstance(parameters[i].ParameterType) : null;
                        }
                        
                        var items = menuItemsMethod.Invoke(null, args) as Array;
                        if (items != null)
                        {
                            Debug.Log($"Found {items.Length} items via Menu.GetMenuItems");
                            
                            foreach (var item in items)
                            {
                                var itemType = item.GetType();
                                var menuPathProp = itemType.GetField("menuPath");
                                var menuPath = menuPathProp?.GetValue(item) as string;
                                
                                if (string.IsNullOrEmpty(menuPath) || menuPath.StartsWith("_"))
                                    continue;
                                
                                AddMenuItemToList(menuItems, menuPath);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in CollectMenuItemsViaReflection: {ex.Message}");
            }
            
            return menuItems;
        }
        
        // Helper method to add a menu item to the list with proper path parsing
        private static void AddMenuItemToList(List<ResponseMenuItem> menuItems, string menuPath)
        {
            // Extract display name (last part of the path after /)
            var displayName = menuPath;
            if (menuPath.Contains('/'))
            {
                displayName = menuPath.Substring(menuPath.LastIndexOf('/') + 1);
            }
            
            // Extract category (first part of the path)
            string category = "";
            if (menuPath.Contains('/'))
            {
                var firstSeparator = menuPath.IndexOf('/');
                category = menuPath.Substring(0, firstSeparator);
            }
            else
            {
                // If there's no separator, the menu path is the category
                category = menuPath;
            }
            
            // Default to enabled
            bool isEnabled = true;
            
            menuItems.Add(new ResponseMenuItem(menuPath, displayName, isEnabled, category));
        }
        
        // Find menu items by scanning for MenuItem attributes in loaded assemblies
        private static List<ResponseMenuItem> FindMenuItemAttributesInAssemblies()
        {
            var result = new List<ResponseMenuItem>();
            
            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                Debug.Log($"Scanning {assemblies.Length} assemblies for MenuItems");
                int foundItemCount = 0;
                
                foreach (var assembly in assemblies)
                {
                    if (assembly.FullName.StartsWith("System.") || 
                        assembly.FullName.StartsWith("Microsoft.") ||
                        assembly.FullName.StartsWith("mscorlib"))
                        continue;
                        
                    try
                    {
                        Type[] types = assembly.GetTypes();
                        
                        foreach (var type in types)
                        {
                            var methods = type.GetMethods(System.Reflection.BindingFlags.Public | 
                                                       System.Reflection.BindingFlags.NonPublic | 
                                                       System.Reflection.BindingFlags.Static);
                            
                            foreach (var method in methods)
                            {
                                var attributes = method.GetCustomAttributes(typeof(MenuItem), false);
                                
                                if (attributes.Length > 0)
                                {
                                    foreach (var attr in attributes)
                                    {
                                        try 
                                        {
                                            MenuItem menuItemAttr = attr as MenuItem;
                                            if (menuItemAttr == null) continue;
                                            
                                            string menuPath = null;
                                            var menuItemProperty = typeof(MenuItem).GetProperty("menuItem", 
                                                System.Reflection.BindingFlags.Public | 
                                                System.Reflection.BindingFlags.NonPublic | 
                                                System.Reflection.BindingFlags.Instance);
                                                
                                            if (menuItemProperty != null)
                                            {
                                                menuPath = menuItemProperty.GetValue(menuItemAttr) as string;
                                            }
                                            else
                                            {
                                                var menuItemField = typeof(MenuItem).GetField("menuItem", 
                                                    System.Reflection.BindingFlags.Public | 
                                                    System.Reflection.BindingFlags.NonPublic | 
                                                    System.Reflection.BindingFlags.Instance);
                                                    
                                                if (menuItemField != null)
                                                {
                                                    menuPath = menuItemField.GetValue(menuItemAttr) as string;
                                                }
                                            }
                                            
                                            if (string.IsNullOrEmpty(menuPath) || menuPath.StartsWith("_"))
                                                continue;
                                            
                                            AddMenuItemToList(result, menuPath);
                                            foundItemCount++;
                                        }
                                        catch (Exception ex)
                                        {
                                            Debug.LogWarning($"Error processing MenuItem attribute: {ex.Message}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Error scanning assembly {assembly.FullName}: {ex.Message}");
                    }
                }
                
                Debug.Log($"Found {foundItemCount} menu items via MenuItem attributes");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error finding MenuItem attributes: {ex.Message}");
            }
            
            return result;
        }
        
        // Fallback list of common menu items
        private static List<ResponseMenuItem> GetCommonMenuItems()
        {
            var items = new List<ResponseMenuItem>();
            
            // Add common menu items
            // File menu
            items.Add(new ResponseMenuItem("File/New Scene", "New Scene", true, "File"));
            items.Add(new ResponseMenuItem("File/Open Scene", "Open Scene", true, "File"));
            items.Add(new ResponseMenuItem("File/Save", "Save", true, "File"));
            items.Add(new ResponseMenuItem("File/Save As...", "Save As...", true, "File"));
            items.Add(new ResponseMenuItem("File/Build Settings...", "Build Settings...", true, "File"));
            items.Add(new ResponseMenuItem("File/Build And Run", "Build And Run", true, "File"));
            items.Add(new ResponseMenuItem("File/Exit", "Exit", true, "File"));
            
            // Edit menu
            items.Add(new ResponseMenuItem("Edit/Undo", "Undo", true, "Edit"));
            items.Add(new ResponseMenuItem("Edit/Redo", "Redo", true, "Edit"));
            items.Add(new ResponseMenuItem("Edit/Cut", "Cut", true, "Edit"));
            items.Add(new ResponseMenuItem("Edit/Copy", "Copy", true, "Edit"));
            items.Add(new ResponseMenuItem("Edit/Paste", "Paste", true, "Edit"));
            items.Add(new ResponseMenuItem("Edit/Duplicate", "Duplicate", true, "Edit"));
            items.Add(new ResponseMenuItem("Edit/Delete", "Delete", true, "Edit"));
            items.Add(new ResponseMenuItem("Edit/Select All", "Select All", true, "Edit"));
            items.Add(new ResponseMenuItem("Edit/Project Settings...", "Project Settings...", true, "Edit"));
            items.Add(new ResponseMenuItem("Edit/Preferences...", "Preferences...", true, "Edit"));
            
            // GameObject menu
            items.Add(new ResponseMenuItem("GameObject/Create Empty", "Create Empty", true, "GameObject"));
            items.Add(new ResponseMenuItem("GameObject/3D Object/Cube", "Cube", true, "GameObject"));
            items.Add(new ResponseMenuItem("GameObject/3D Object/Sphere", "Sphere", true, "GameObject"));
            items.Add(new ResponseMenuItem("GameObject/3D Object/Capsule", "Capsule", true, "GameObject"));
            items.Add(new ResponseMenuItem("GameObject/3D Object/Cylinder", "Cylinder", true, "GameObject"));
            items.Add(new ResponseMenuItem("GameObject/3D Object/Plane", "Plane", true, "GameObject"));
            items.Add(new ResponseMenuItem("GameObject/Light/Directional Light", "Directional Light", true, "GameObject"));
            items.Add(new ResponseMenuItem("GameObject/Light/Point Light", "Point Light", true, "GameObject"));
            items.Add(new ResponseMenuItem("GameObject/Light/Spot Light", "Spot Light", true, "GameObject"));
            items.Add(new ResponseMenuItem("GameObject/Camera", "Camera", true, "GameObject"));
            
            // Component menu
            items.Add(new ResponseMenuItem("Component/Mesh/Mesh Filter", "Mesh Filter", true, "Component"));
            items.Add(new ResponseMenuItem("Component/Mesh/Mesh Renderer", "Mesh Renderer", true, "Component"));
            items.Add(new ResponseMenuItem("Component/Physics/Rigidbody", "Rigidbody", true, "Component"));
            items.Add(new ResponseMenuItem("Component/Physics/Box Collider", "Box Collider", true, "Component"));
            items.Add(new ResponseMenuItem("Component/Physics/Sphere Collider", "Sphere Collider", true, "Component"));
            items.Add(new ResponseMenuItem("Component/Physics/Capsule Collider", "Capsule Collider", true, "Component"));
            
            // Window menu
            items.Add(new ResponseMenuItem("Window/Package Manager", "Package Manager", true, "Window"));
            items.Add(new ResponseMenuItem("Window/Animation/Animation", "Animation", true, "Window"));
            items.Add(new ResponseMenuItem("Window/Animation/Animator", "Animator", true, "Window"));
            items.Add(new ResponseMenuItem("Window/Audio/Audio Mixer", "Audio Mixer", true, "Window"));
            items.Add(new ResponseMenuItem("Window/General/Scene", "Scene", true, "Window"));
            items.Add(new ResponseMenuItem("Window/General/Game", "Game", true, "Window"));
            items.Add(new ResponseMenuItem("Window/General/Inspector", "Inspector", true, "Window"));
            items.Add(new ResponseMenuItem("Window/General/Hierarchy", "Hierarchy", true, "Window"));
            items.Add(new ResponseMenuItem("Window/General/Project", "Project", true, "Window"));
            items.Add(new ResponseMenuItem("Window/General/Console", "Console", true, "Window"));
            
            // Tools menu
            items.Add(new ResponseMenuItem("Tools/AI Connector (Unity-MCP)/Test/List Top-Level Menus", "List Top-Level Menus", true, "Tools"));
            items.Add(new ResponseMenuItem("Tools/AI Connector (Unity-MCP)/Test/List File Menu Items", "List File Menu Items", true, "Tools"));
            items.Add(new ResponseMenuItem("Tools/AI Connector (Unity-MCP)/Test/List GameObject Menu Items", "List GameObject Menu Items", true, "Tools"));
            items.Add(new ResponseMenuItem("Tools/AI Connector (Unity-MCP)/Test/Execute Menu Item", "Execute Menu Item", true, "Tools"));
            
            Debug.Log($"Added {items.Count} common menu items as fallback");
            return items;
        }
        
        public static ResponseExecuteMenuItem ExecuteMenuItem(string menuPath)
        {
            try
            {
                if (string.IsNullOrEmpty(menuPath))
                {
                    return new ResponseExecuteMenuItem(menuPath, false, "Menu path cannot be empty");
                }
                
                // Make sure our cache is initialized
                if (_allMenuItems == null)
                {
                    _allMenuItems = GetAllMenuItems();
                }
                
                // Try to execute the menu item via EditorApplication
                try
                {
                    EditorApplication.ExecuteMenuItem(menuPath);
                    return new ResponseExecuteMenuItem(menuPath, true, "Menu item executed successfully");
                }
                catch (Exception ex)
                {
                    // Log the error and return a failure response
                    Debug.LogError($"Error executing menu item '{menuPath}': {ex.Message}");
                    return new ResponseExecuteMenuItem(menuPath, false, $"Error executing menu item: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                return new ResponseExecuteMenuItem(menuPath, false, $"Error executing menu item: {ex.Message}");
            }
        }
    }
#else
    public class MenuItemService
    {
        public static ResponseMenuItem[] GetMenuItems(string parentPath = "")
        {
            return new ResponseMenuItem[0]; // Return empty array for runtime builds
        }
        
        public static ResponseMenuItem[] GetAllMenuItemsArray()
        {
            return new ResponseMenuItem[0]; // Return empty array for runtime builds
        }
        
        public static ResponseExecuteMenuItem ExecuteMenuItem(string menuPath)
        {
            return new ResponseExecuteMenuItem(menuPath, false, "Menu item execution only available in Unity Editor");
        }
    }
#endif
} 