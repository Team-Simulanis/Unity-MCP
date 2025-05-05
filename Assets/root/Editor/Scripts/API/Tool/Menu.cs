using com.IvanMurzak.Unity.MCP.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.IvanMurzak.Unity.MCP.Common.Data;
using com.IvanMurzak.Unity.MCP.Unity;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    [McpPluginToolType]
    public partial class Tool_Menu
    {
        // Base class for menu-related tools

        // Common menu paths with descriptions for reference
        private static readonly Dictionary<string, string> MenuPathDescriptions = new Dictionary<string, string>
        {
            { "Window/AI Connector (Unity-MCP)", "Opens the AI Connector window for Unity-MCP" },
            { "Tools/AI Connector (Unity-MCP)/Build MCP Server", "Builds the MCP server" },
            { "Tools/AI Connector (Unity-MCP)/Open Server Logs", "Opens the server logs" },
            { "File/New Scene %n", "Creates a new empty scene" },
            { "File/Save", "Saves the current scene" },
            { "File/Save Project", "Saves the entire project" },
            { "Edit/Duplicate", "Duplicates the selected objects" },
            { "Edit/Delete", "Deletes the selected objects" },
            { "GameObject/Create Empty", "Creates an empty GameObject" },
            { "Window/Package Manager", "Opens the Package Manager" },
            { "Window/Console", "Opens the Console window" },
            { "Window/Scene", "Opens the Scene view" },
            { "Assets/Create/Material", "Creates a new material" },
            { "Assets/Create/Prefab", "Creates a new prefab" }
        };
    }
} 