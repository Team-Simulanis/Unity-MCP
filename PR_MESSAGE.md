# Add Menu_ExecuteItem Tool for Unity Menu Integration

## Description
This PR adds a new tool that allows LLMs to execute Unity menu items directly. This enables AI agents to interact with Unity's editor menu system, making it possible to trigger menu actions through natural language commands.

## Features
- New `Menu_ExecuteItem` tool that executes Unity menu items by path
- Simple response format with clear success/failure indication
- Comprehensive error handling
- Well-documented parameter descriptions for LLM understanding using System.ComponentModel.Description

## Implementation Details
The implementation includes:
1. A `Menu.cs` base class for menu-related tools
2. The `Menu.ExecuteItem.cs` implementation file
3. Supporting service classes:
   - `MenuItemService.cs` for finding and executing menu items
   - `MenuItemResponses.cs` for structured response data
   - `MenuItemHelper.cs` for utility functions

## Examples of Use
An LLM can call the tool like this:
```
Menu_ExecuteItem("Window/Console")
```

Which will:
1. Execute the menu command to open the Console window
2. Return a success/failure message with details

## Use Cases
This tool enables LLMs to:
- Open specific editor windows
- Trigger build processes
- Save scenes and projects
- Create new assets through menu commands
- Access any functionality exposed through Unity's menu system

## Compatibility Note
This PR uses only the existing `McpPluginToolAttribute` properties (Name, Title) and adds separate `System.ComponentModel.Description` attributes for documentation. No changes to the core attribute classes were required. 