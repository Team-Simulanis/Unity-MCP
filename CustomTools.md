# Unity-MCP Custom Tools Documentation

## Understanding Menu Tools Implementation

This document explains the implementation of custom tools in Unity-MCP, specifically focusing on the menu listing and execution tools we've added. It serves as a reference for adding new tools in the future.

## 1. Architecture Overview

The Unity-MCP system exposes functionality to LLMs through a tool-based architecture:

- **Tools**: Methods that can be called by the LLM
- **Tool Types**: Classes that contain tool methods
- **Attributes**: Used to mark classes and methods as tools

## 2. Implemented Menu Tools

We've implemented two main menu-related tools:

1. **Menu_ListItems**: Lists available Unity menu items, either top-level categories or items within a specific path
2. **Menu_ExecuteItem**: Executes a specified menu item

## 3. Code Structure

### 3.1. Tool Class Definition

The tools are implemented in a partial class called `Tool_Menu`:

```csharp
// Assets/root/Editor/Scripts/API/Tool/Menu.cs
namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    [McpPluginToolType]
    public partial class Tool_Menu
    {
        // Tool methods implemented in partial class files
    }
}
```

Key points:
- The `[McpPluginToolType]` attribute marks this class as a container for tools
- Using a partial class allows separating different tool implementations into dedicated files

### 3.2. Menu_ListItems Implementation

```csharp
// Assets/root/Editor/Scripts/API/Tool/Menu.ListItems.cs
[McpPluginTool
(
    "Menu_ListItems",
    Title = "List Unity Menu Items",
    Description = "List available Unity menu items. Provide a parent path to see child items."
)]
public string ListItems(
    [System.ComponentModel.Description("Parent menu path. Empty for top-level menus.")]
    string parentPath = "")
{
    Debug.Log($"[MCP] Menu.ListItems called with parentPath: {parentPath}");
    
    return MainThread.Run(() =>
    {
        try
        {
            // Get menu items using the MenuItemService
            var menuItems = MenuItemService.GetMenuItems(parentPath);
            
            // Format the response as a nice markdown table
            // ...
            
            return result.ToString();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error listing menus: {ex.Message}\n{ex.StackTrace}");
            return $"Error listing menus: {ex.Message}";
        }
    });
}
```

Key points:
- `[McpPluginTool]` attribute defines the tool name, title, and description
- `[Description]` attribute on parameters helps the LLM understand what the parameter does
- `MainThread.Run()` ensures the code runs on Unity's main thread
- The tool returns formatted text that's easy for the LLM to understand

### 3.3. Menu_ExecuteItem Implementation

```csharp
// Assets/root/Editor/Scripts/API/Tool/Menu.ExecuteItem.cs
[McpPluginTool
(
    "Menu_ExecuteItem",
    Title = "Execute Menu Item",
    Description = "Execute a Unity menu item by path"
)]
public string ExecuteItem
(
    [System.ComponentModel.Description("Path to the menu item to execute")]
    string menuPath = "Window/AI Connector (Unity-MCP)"
)
{
    Debug.Log($"[MCP] Menu.ExecuteItem called with menuPath: {menuPath}");
    
    return MainThread.Run(() =>
    {
        try
        {
            // Execute the menu item using MenuItemService
            var result = MenuItemService.ExecuteMenuItem(menuPath);
            
            // Format the response based on success
            string response;
            if (result.Success)
            {
                response = $"✅ Successfully executed menu item: {menuPath}";
            }
            else
            {
                response = $"❌ Failed to execute menu item: {menuPath}\nMessage: {result.Message}";
            }
            
            return response;
        }
        catch (Exception ex)
        {
            return $"⚠️ Exception executing menu: {ex.Message}";
        }
    });
}
```

Key points:
- Similar structure to the ListItems tool
- Provides clear success/failure feedback with emoji for better readability
- Uses a default value for the menuPath parameter

### 3.4. Supporting Service Classes

The tools rely on a `MenuItemService` class that handles the actual work:

```csharp
// Assets/root/Runtime/Unity/MenuItemService.cs
public class MenuItemService
{
    // Cache menu items
    private static List<ResponseMenuItem> _allMenuItems;
    
    // Get menu items by parent path
    public static ResponseMenuItem[] GetMenuItems(string parentPath = "") { ... }
    
    // Execute a menu item
    public static ResponseExecuteMenuItem ExecuteMenuItem(string menuPath) { ... }
    
    // Helper methods for collecting menu items
    private static List<ResponseMenuItem> GetAllMenuItems() { ... }
    private static List<ResponseMenuItem> CollectMenuItemsViaReflection() { ... }
    private static List<ResponseMenuItem> FindMenuItemAttributesInAssemblies() { ... }
}
```

And response data classes for structured returns:

```csharp
// Assets/root/Server/Common/Data/Response/MenuItemResponses.cs
public class ResponseMenuItem
{
    public string MenuPath { get; set; }
    public string DisplayName { get; set; }
    public bool IsEnabled { get; set; }
    public string Category { get; set; }
}

public class ResponseExecuteMenuItem
{
    public string MenuPath { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; }
}
```

## 4. Adding a New Tool

To add a new tool to Unity-MCP:

1. **Create or identify a tool class**:
   - Use an existing class like `Tool_Menu` for related functionality
   - Or create a new class with the `[McpPluginToolType]` attribute

2. **Add a new method with the tool attribute**:
   ```csharp
   [McpPluginTool(
       "YourTool_Name",
       Title = "Human-readable title",
       Description = "Detailed description for the LLM"
   )]
   public string YourMethodName(
       [System.ComponentModel.Description("Parameter description")]
       string paramName = "defaultValue"
   )
   {
       return MainThread.Run(() => {
           // Your implementation here
           return "Result to be shown to the user";
       });
   }
   ```

3. **Use proper parameter descriptions**:
   - Add `[Description]` attributes to all parameters
   - Use nullable types with default values for optional parameters
   - Consider using descriptive parameter names

4. **Include proper error handling**:
   - Wrap implementation in try/catch blocks
   - Return user-friendly error messages
   - Log details for debugging

5. **Format the output for LLM readability**:
   - Use markdown for structured output
   - Include emojis for quick visual feedback
   - Provide clear success/error indicators

## 5. Important Considerations

1. **Thread Safety**: Always use `MainThread.Run()` for Unity API calls
2. **Performance**: Cache results when possible (like the menu items cache)
3. **Compatibility**: Consider runtime vs editor-only functionality
4. **Documentation**: Provide clear descriptions for both the tool and its parameters
5. **User Experience**: Format outputs to be easily understood by both the LLM and end-user
6. **Error Handling**: Ensure tools gracefully handle errors and provide useful feedback

## Conclusion

Our custom menu tools demonstrate how to extend Unity-MCP with new functionality. By following this pattern, you can create additional tools that expose other Unity functionality to LLMs in a structured, reliable way. 