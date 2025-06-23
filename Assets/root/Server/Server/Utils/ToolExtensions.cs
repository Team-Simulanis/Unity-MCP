#if !UNITY_5_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.ReflectorNet.Model;
using ModelContextProtocol.Protocol;

namespace com.IvanMurzak.Unity.MCP.Server
{

    public static class ToolsExtensions
    {
        public static CallToolResult SetError(this CallToolResult target, string message)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            target.IsError = true;
            target.Content ??= new List<ContentBlock>(1);
            target.Content.Add(new TextContentBlock()
            {
                Type = "text",
                Text = message
            });

            return target;
        }

        public static ListToolsResult SetError(this ListToolsResult target, string message)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            target.Tools = new List<Tool>();

            return target;
        }

        public static Tool ToTool(this IResponseListTool response) => new Tool()
        {
            Name = response.Name,
            Description = response.Description,
            InputSchema = response.InputSchema,
            Annotations = new()
            {
                Title = response.Title
            },
        };

        public static CallToolResult ToCallToolResult(this IResponseCallTool response) => new CallToolResult()
        {
            IsError = response.IsError,
            Content = response.Content
                .Select(x => x.ToContent())
                .ToList()
        };

        public static ContentBlock ToContent(this ResponseCallToolContent response) => new TextContentBlock()
        {
            Type = response.Type,
            // MimeType = response.MimeType,
            Text = response.Text ?? string.Empty
            // Data = response.Data
        };
    }
}
#endif