using Microsoft.Extensions.Logging;
using System;

namespace com.IvanMurzak.Unity.MCP.Server.Utils
{
    /// <summary>
    /// Provides standardized logging utilities for Unity-MCP tools.
    /// </summary>
    public static class ToolLogger
    {
        /// <summary>
        /// The verbosity level for tool logging.
        /// </summary>
        public enum VerbosityLevel
        {
            /// <summary>Minimal logging, errors only</summary>
            Minimal = 0,
            
            /// <summary>Normal logging, includes errors and important operations</summary>
            Normal = 1,
            
            /// <summary>Verbose logging, includes all details about tool operations</summary>
            Verbose = 2
        }

        /// <summary>
        /// The current verbosity level for logging.
        /// </summary>
        public static VerbosityLevel CurrentVerbosity { get; set; } = VerbosityLevel.Normal;

        /// <summary>
        /// Logs the successful execution of a tool.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="toolName">The name of the tool.</param>
        /// <param name="message">The success message.</param>
        public static void LogToolSuccess(ILogger logger, string toolName, string message)
        {
            if (CurrentVerbosity >= VerbosityLevel.Normal)
            {
                logger.LogInformation($"SUCCESS: Tool '{toolName}' - {message}");
            }
        }

        /// <summary>
        /// Logs the failure of a tool execution.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="toolName">The name of the tool.</param>
        /// <param name="message">The error message.</param>
        /// <param name="exception">Optional exception that caused the failure.</param>
        public static void LogToolError(ILogger logger, string toolName, string message, Exception exception = null)
        {
            // Always log errors regardless of verbosity
            if (exception != null)
            {
                logger.LogError(exception, $"ERROR: Tool '{toolName}' - {message}");
            }
            else
            {
                logger.LogError($"ERROR: Tool '{toolName}' - {message}");
            }
        }

        /// <summary>
        /// Logs detailed information about a tool execution.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="toolName">The name of the tool.</param>
        /// <param name="message">The detail message.</param>
        public static void LogToolDetail(ILogger logger, string toolName, string message)
        {
            if (CurrentVerbosity >= VerbosityLevel.Verbose)
            {
                logger.LogDebug($"INFO: Tool '{toolName}' - {message}");
            }
        }

        /// <summary>
        /// Logs the start of a tool execution.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="toolName">The name of the tool.</param>
        /// <param name="parameters">Optional parameter information.</param>
        public static void LogToolStart(ILogger logger, string toolName, string parameters = null)
        {
            if (CurrentVerbosity >= VerbosityLevel.Normal)
            {
                if (string.IsNullOrEmpty(parameters))
                {
                    logger.LogInformation($"START: Tool '{toolName}' execution");
                }
                else
                {
                    logger.LogInformation($"START: Tool '{toolName}' execution with parameters: {parameters}");
                }
            }
        }
    }
} 