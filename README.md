# Simulanis Unity MCP (Server + Plugin)

> This is a fork of [Unity-MCP](https://github.com/IvanMurzak/Unity-MCP) by [Ivan Murzak](https://github.com/IvanMurzak), maintained by Simulanis Solutions.

[![openupm](https://img.shields.io/npm/v/com.simulanis.unity.mcp?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.simulanis.unity.mcp/) ![License](https://img.shields.io/github/license/IvanMurzak/Unity-MCP) [![Stand With Ukraine](https://raw.githubusercontent.com/vshymanskyy/StandWithUkraine/main/badges/StandWithUkraine.svg)](https://stand-with-ukraine.pp.ua)

[![GitHub release (latest by date)](https://img.shields.io/github/v/release/Team-Simulanis/Unity-MCP)](https://github.com/Team-Simulanis/Unity-MCP/releases/latest) ![License](https://img.shields.io/github/license/IvanMurzak/Unity-MCP)

![image](https://github.com/user-attachments/assets/8f595879-a578-421a-a06d-8c194af874f7)

| Unity Version | Editmode | Playmode | Standalone |
|---------------|----------|----------|------------|
| 2022.3.61f1   | ![2022.3.61f1](https://img.shields.io/github/actions/workflow/status/IvanMurzak/Unity-MCP/2022.3.61f1_editmode.yml?label=2022.3.61f1-editmode) | ![2022.3.61f1](https://img.shields.io/github/actions/workflow/status/IvanMurzak/Unity-MCP/2022.3.61f1_playmode.yml?label=2022.3.61f1-playmode) | ![2022.3.61f1](https://img.shields.io/github/actions/workflow/status/IvanMurzak/Unity-MCP/2022.3.61f1_standalone.yml?label=2022.3.61f1-standalone) |
| 2023.2.20f1   | ![2023.2.20f1](https://img.shields.io/github/actions/workflow/status/IvanMurzak/Unity-MCP/2023.2.20f1_editmode.yml?label=2023.2.20f1-editmode) | ![2023.2.20f1](https://img.shields.io/github/actions/workflow/status/IvanMurzak/Unity-MCP/2023.2.20f1_playmode.yml?label=2023.2.20f1-playmode) | ![2023.2.20f1](https://img.shields.io/github/actions/workflow/status/IvanMurzak/Unity-MCP/2023.2.20f1_standalone.yml?label=2023.2.20f1-standalone) |
| 6000.0.46f1   | ![6000.0.46f1](https://img.shields.io/github/actions/workflow/status/IvanMurzak/Unity-MCP/6000.0.46f1_editmode.yml?label=6000.0.46f1-editmode) | ![6000.0.46f1](https://img.shields.io/github/actions/workflow/status/IvanMurzak/Unity-MCP/6000.0.46f1_playmode.yml?label=6000.0.46f1-playmode) | ![6000.0.46f1](https://img.shields.io/github/actions/workflow/status/IvanMurzak/Unity-MCP/6000.0.46f1_standalone.yml?label=6000.0.46f1-standalone) |

## About This Fork

This fork is maintained by Simulanis Solutions to provide enhanced Unity-MCP functionality for Simulanis projects while maintaining compatibility with the original project. We are committed to:

- Regular synchronization with the upstream repository
- Maintaining proper attribution to the original work
- Contributing improvements back to the original project when possible

## Original Project Description

**[Unity-MCP](https://github.com/IvanMurzak/Unity-MCP)** is a bridge between LLM and Unity. It exposes and explains to LLM Unity's tools. LLM understands the interface and utilizes the tools in the way a user asks.

Connect **[Unity-MCP](https://github.com/IvanMurzak/Unity-MCP)** to LLM client such as [Claude](https://claude.ai/download) or [Cursor](https://www.cursor.com/) using integrated `AI Connector` window. Custom clients are supported as well.

The project is designed to let developers to add custom tools soon. After that the next goal is to enable the same features in player's build. For not it works only in Unity Editor.

The system is extensible: you can define custom `tool`s directly in your Unity project codebase, exposing new capabilities to the AI or automation clients. This makes Unity-MCP a flexible foundation for building advanced workflows, rapid prototyping, or integrating AI-driven features into your development process.

## AI Tools

<table>
<tr>
<td valign="top">

### GameObject

- ✅ Create
- ✅ Destroy
- ✅ Find
- 🔲 Modify (tag, layer, name, static)
- ✅ Set parent
- ✅ Duplicate

##### GameObject.Components

- ✅ Add Component
- ✅ Get Components
- ✅ Modify Component
- - ✅ `Field` set value
- - ✅ `Property` set value
- - ✅ `Reference` link set
- ✅ Destroy Component
- 🔲 Remove missing components

### Editor

- ✅ State (Playmode)
  - ✅ Get
  - ✅ Set
- 🔲 Get Windows
- 🔲 Layer
  - 🔲 Get All
  - 🔲 Add
  - 🔲 Remove
- 🔲 Tag
  - 🔲 Get All
  - 🔲 Add
  - 🔲 Remove
- 🔲 Execute `MenuItem`
- 🔲 Run Tests

#### Editor.Selection

- ✅ Get selection
- ✅ Set selection

### Prefabs

- ✅ Instantiate
- 🔲 Create
- ✅ Open
- ✅ Modify (GameObject.Modify)
- ✅ Save
- ✅ Close

</td>
<td valign="top">

### Assets

- ✅ Create
- ✅ Find
- ✅ Refresh
- 🔲 Import (is it needed?)
- ✅ Read
- ✅ Modify
- ✅ Rename
- ✅ Delete
- ✅ Move
- ✅ Create folder

### Scene

- ✅ Create
- ✅ Save
- ✅ Load
- ✅ Unload
- ✅ Get Loaded
- ✅ Get hierarchy
- 🔲 Search (editor)
- 🔲 Raycast (understand volume)

### Materials

- ✅ Create
- ✅ Modify (Assets.Modify)
- ✅ Read (Assets.Read)
- ✅ Assign to a Component on a GameObject

### Shader

- ✅ List All

### Scripts

- ✅ Read
- ✅ Update or Create
- ✅ Delete

### Scriptable Object

- 🔲 Create
- 🔲 Read
- 🔲 Modify
- 🔲 Remove

### Debug

- 🔲 Read logs (console)

### Component

- ✅ Get All

### Package

- 🔲 Get installed
- 🔲 Install
- 🔲 Remove
- 🔲 Update

</td>
</tr>
</table>

> **Legend:**
> ✅ = Implemented & available, 🔲 = Planned / Not yet implemented

# Installation (via Git URL)

This package is distributed directly via GitHub using its Git URL and release tags.

1.  **Prerequisites:**
    *   Unity 2022.3 or newer.
    *   [.NET 9.0 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/9.0) installed.
    *   Git installed and available in your system's PATH.

2.  **Add Package via Unity Package Manager:**
    *   Open your Unity project.
    *   Go to `Window > Package Manager`.
    *   Click the `+` (plus) button in the top-left corner of the Package Manager window.
    *   Select `Add package from git URL...`.
    *   Enter the following URL, replacing `TAG_NAME` with the desired release tag (e.g., `v0.7.3-simulanis.3`) and including the `?path=` parameter:
        ```
        https://github.com/Team-Simulanis/Unity-MCP.git?path=Assets/root#TAG_NAME
        ```
        For the latest release (`v0.7.3-simulanis.3`), use:
        ```
        https://github.com/Team-Simulanis/Unity-MCP.git?path=Assets/root#v0.7.3-simulanis.3
        ```
    *   Click `Add`.
    *   Unity will download and import the package from the specified path within the repository.

3.  **Ensure Scoped Registry for Dependencies:**
    *   After installation, check your `Packages/manifest.json` file.
    *   Make sure the following `scopedRegistries` entry exists to allow Unity to download necessary dependencies (like SignalR components):
        ```json
        {
          "dependencies": {
            // ... other dependencies, including com.simulanis.unity.mcp
          },
          "scopedRegistries": [ 
            {
              "name": "package.openupm.com",
              "url": "https://package.openupm.com",
              "scopes": [
                "org.nuget"
              ]
            }
            // ... other scoped registries if you have them
          ]
        }
        ```
    *   If it's missing, add it manually and save the `manifest.json` file. Unity should then resolve the remaining dependencies.

4.  **Build Server:**
    *   Once the package and its dependencies are imported, go to the Unity menu: `Tools > AI Connector (Unity-MCP) > Build MCP Server`.
    *   Click it to build the necessary server component. Check the Unity console for success messages.

# Usage

1. Make sure your project path doesn't have a space symbol " ".
> - ✅ `C:/MyProjects/Project`
> - ❌ `C:/My Projects/Project`

2. Open Unity project, go 👉 `Window/AI Connector (Unity-MCP)`.

![Unity_WaSRb5FIAR](https://github.com/user-attachments/assets/e8049620-6614-45f1-92d7-cc5d00a6b074)

3. Install MCP client
> - [Install Cursor](https://www.cursor.com/) (recommended)
> - [Install Claude](https://claude.ai/download)

4. Sign-in into MCP client
5. Click `Configure` at your MCP client.

![image](https://github.com/user-attachments/assets/19f80179-c5b3-4e9c-bdf6-07edfb773018)

6. Restart your MCP client.
7. Make sure `AI Connector` is "Connected" or "Connecting..." after restart.
8. Test AI connection in your Client (Cursor, Claude Desktop). Type any question or task into the chat. Something like:

  ```text
  Explain my scene hierarchy
  ```

# Support

For issues specific to the Simulanis fork or integration with Simulanis projects, please contact yokeshj@simulanis.com or raise an issue on this GitHub repository.

For issues related to the core MCP functionality, please refer to the original [Unity-MCP issues section](https://github.com/IvanMurzak/Unity-MCP/issues).

# Add custom `tool`

> ⚠️ It only works with MCP client that supports dynamic tool list update.

Unity-MCP is designed to support custom `tool` development by project owner. MCP server takes data from Unity plugin and exposes it to a Client. So anyone in the MCP communication chain would receive the information about a new `tool`. Which LLM may decide to call at some point.

To add a custom `tool` you need:

1. To have a class with attribute `McpPluginToolType`.
2. To have a method in the class with attribute `McpPluginTool`.
3. [optional] Add `Description` attribute to each method argument to let LLM to understand it.
4. [optional] Use `string? optional = null` properties with `?` and default value to mark them as `optional` for LLM.

> Take a look that the line `MainThread.Run(() =>` it allows to run the code in Main thread which is needed to interact with Unity API. If you don't need it and running the tool in background thread is fine for the tool, don't use Main thread for efficiency purpose.

```csharp
[McpPluginToolType]
public class Tool_GameObject
{
    [McpPluginTool
    (
        "MyCustomTask",
        Title = "Create a new GameObject"
    )]
    [Description("Explain here to LLM what is this, when it should be called.")]
    public string CustomTask
    (
        [Description("Explain to LLM what is this.")]
        string inputData
    )
    {
        // do anything in background thread

        return MainThread.Run(() =>
        {
            // do something in main thread if needed

            return $"[Success] Operation completed.";
        });
    }
}
```

# Add custom in-game `tool`

> ⚠️ Not yet supported. The work is in progress


# Contribution

Feel free to add new `tool` into the project.

1. Fork the project.
2. Implement new `tool` in your forked repository.
3. Create Pull Request into original [Unity-MCP](https://github.com/IvanMurzak/Unity-MCP) repository.
