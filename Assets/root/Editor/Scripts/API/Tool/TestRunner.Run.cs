#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_TestRunner
    {
        private static bool _isTestRunning;
        private static readonly object _testLock = new();

        [McpPluginTool
        (
            "TestRunner_Run",
            Title = "Run Unity Tests"
        )]
        [Description("Execute Unity tests and return detailed results. Supports filtering by test mode, assembly, namespace, class, and method.")]
        public async Task<string> Run
        (
            [Description("Test mode to run. Options: 'EditMode', 'PlayMode', 'All'. Default: 'All'")]
            string testMode = "All",
            [Description("Specific test assembly name to run (optional). Example: 'Assembly-CSharp-Editor-testable'")]
            string? testAssembly = null,
            [Description("Specific test namespace to run (optional). Example: 'MyTestNamespace'")]
            string? testNamespace = null,
            [Description("Specific test class name to run (optional). Example: 'MyTestClass'")]
            string? testClass = null,
            [Description("Specific fully qualified test method to run (optional). Example: 'MyTestNamespace.FixtureName.TestName'")]
            string? testMethod = null
        )
        {
            try
            {
                // Check if tests are already running
                lock (_testLock)
                {
                    if (_isTestRunning)
                    {
                        return "[Error] Test execution is already in progress. Please wait for the current test run to complete.";
                    }
                    _isTestRunning = true;
                }

                // Validate test mode
                if (!IsValidTestMode(testMode))
                    return Error.InvalidTestMode(testMode);

                // Get timeout from MCP server configuration
                var timeoutMs = McpPluginUnity.TimeoutMs;
                Debug.Log($"[TestRunner] Using timeout: {timeoutMs} ms (from MCP plugin configuration)");

                // Get Test Runner API (must be on main thread)
                var testRunnerApi = await MainThread.Instance.RunAsync(() => ScriptableObject.CreateInstance<TestRunnerApi>());
                if (testRunnerApi == null)
                    return Error.TestRunnerNotAvailable();

                if (testMode == "All")
                {
                    // Create filter parameters
                    var filterParams = new TestFilterParameters(testAssembly, testNamespace, testClass, testMethod);

                    // Check which modes have matching tests
                    var editModeTestCount = await GetMatchingTestCount(testRunnerApi, TestMode.EditMode, filterParams);
                    var playModeTestCount = await GetMatchingTestCount(testRunnerApi, TestMode.PlayMode, filterParams);

                    // If neither mode has tests, return error
                    if (editModeTestCount == 0 && playModeTestCount == 0)
                    {
                        return Error.NoTestsFound(filterParams);
                    }

                    // Handle "All" mode by running only the modes that have matching tests
                    var modesToRun = new List<string>();
                    if (editModeTestCount > 0) modesToRun.Add("EditMode");
                    if (playModeTestCount > 0) modesToRun.Add("PlayMode");

                    Debug.Log($"[TestRunner] Running tests in modes: {string.Join(", ", modesToRun)} (EditMode: {editModeTestCount}, PlayMode: {playModeTestCount})");
                    return await RunSequentialTests(testRunnerApi, filterParams, timeoutMs, editModeTestCount > 0, playModeTestCount > 0);
                }
                else
                {
                    // Create filter parameters
                    var filterParams = new TestFilterParameters(testAssembly, testNamespace, testClass, testMethod);

                    // Convert string to TestMode enum
                    var testModeEnum = testMode == "EditMode"
                        ? TestMode.EditMode
                        : TestMode.PlayMode;

                    // Validate specific test mode filter
                    var validation = await ValidateTestFilters(testRunnerApi, testModeEnum, filterParams);
                    if (validation != null)
                        return validation;

                    Debug.Log($"[TestRunner] Running {testMode} tests.");
                    var resultCollector = await RunSingleTestModeWithCollector(testModeEnum, testRunnerApi, filterParams, timeoutMs);
                    return FormatTestResults(resultCollector);
                }
            }
            catch (OperationCanceledException)
            {
                return Error.TestTimeout(McpPluginUnity.TimeoutMs);
            }
            catch (Exception ex)
            {
                return Error.TestExecutionFailed(ex.Message);
            }
            finally
            {
                // Always release the lock when done
                lock (_testLock)
                {
                    _isTestRunning = false;
                }
            }
        }

        private static bool IsValidTestMode(string testMode)
        {
            return testMode switch
            {
                "EditMode" => true,
                "PlayMode" => true,
                "All" => true,
                _ => false
            };
        }

        private static Filter CreateTestFilter(TestMode testMode, TestFilterParameters filterParams)
        {
            var filter = new Filter
            {
                testMode = testMode
            };

            if (!string.IsNullOrEmpty(filterParams.TestAssembly))
            {
                filter.assemblyNames = new[] { filterParams.TestAssembly };
            }

            var groupNames = new List<string>();
            var testNames = new List<string>();

            // Handle specific test method in FixtureName.TestName format
            if (!string.IsNullOrEmpty(filterParams.TestMethod))
            {
                testNames.Add(filterParams.TestMethod);
            }

            // Handle namespace filtering with regex (shared pattern ensures validation sync)
            if (!string.IsNullOrEmpty(filterParams.TestNamespace))
            {
                groupNames.Add(CreateNamespaceRegexPattern(filterParams.TestNamespace));
            }

            // Handle class filtering with regex (shared pattern ensures validation sync)
            if (!string.IsNullOrEmpty(filterParams.TestClass))
            {
                groupNames.Add(CreateClassRegexPattern(filterParams.TestClass));
            }

            if (groupNames.Any())
            {
                filter.groupNames = groupNames.ToArray();
            }

            if (testNames.Any())
            {
                filter.testNames = testNames.ToArray();
            }

            return filter;
        }

        /// <summary>
        /// Creates a regex pattern for namespace filtering that matches Unity's Filter.groupNames behavior.
        /// This ensures our validation logic (CountFilteredTests) matches exactly what Unity's TestRunner will execute.
        /// Pattern: "^{namespace}\." - matches tests in the specified namespace and its subnamespaces.
        /// </summary>
        /// <param name="namespaceName">The namespace to filter by</param>
        /// <returns>Regex pattern for Unity's Filter.groupNames field</returns>
        private static string CreateNamespaceRegexPattern(string namespaceName)
        {
            return $"^{EscapeRegex(namespaceName)}\\.";
        }

        /// <summary>
        /// Creates a regex pattern for class filtering that matches Unity's Filter.groupNames behavior.
        /// This ensures our validation logic (CountFilteredTests) matches exactly what Unity's TestRunner will execute.
        /// Pattern: "^.*\.{className}$" - matches any test class with the specified name regardless of namespace.
        /// </summary>
        /// <param name="className">The class name to filter by</param>
        /// <returns>Regex pattern for Unity's Filter.groupNames field</returns>
        private static string CreateClassRegexPattern(string className)
        {
            return $"^.*\\.{EscapeRegex(className)}$";
        }

        /// <summary>
        /// Escapes special regex characters to ensure literal string matching.
        /// Used by the shared regex pattern builders to safely handle user input that may contain regex metacharacters.
        /// </summary>
        /// <param name="input">The string to escape</param>
        /// <returns>Regex-safe escaped string</returns>
        private static string EscapeRegex(string input)
        {
            return System.Text.RegularExpressions.Regex.Escape(input);
        }

        private async Task<int> GetMatchingTestCount(TestRunnerApi testRunnerApi, TestMode testMode, TestFilterParameters filterParams)
        {
            try
            {
                var tcs = new TaskCompletionSource<int>();

                // Retrieve test list without running tests
                await MainThread.Instance.RunAsync(() =>
                {
                    testRunnerApi.RetrieveTestList(testMode, (testRoot) =>
                    {
                        var testCount = testRoot != null
                            ? CountFilteredTests(testRoot, filterParams)
                            : 0;
                        Debug.Log($"[TestRunner] {testCount} {testMode} tests matched for {filterParams}");
                        tcs.SetResult(testCount);
                    });
                });

                // Wait for the test count result with timeout
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));
                var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    throw new OperationCanceledException("Test list retrieval timed out");
                }

                return await tcs.Task;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private async Task<string?> ValidateTestFilters(TestRunnerApi testRunnerApi, TestMode testMode, TestFilterParameters filterParams)
        {
            try
            {
                var testCount = await GetMatchingTestCount(testRunnerApi, testMode, filterParams);

                if (testCount == 0)
                {
                    return Error.NoTestsFound(filterParams);
                }

                return null; // No error, tests found
            }
            catch (Exception ex)
            {
                return Error.TestExecutionFailed($"Filter validation failed: {ex.Message}");
            }
        }

        private static int CountFilteredTests(ITestAdaptor test, TestFilterParameters filterParams)
        {
            // If no filters are specified, count all tests
            if (!filterParams.HasAnyFilter)
            {
                return TestResultCollector.CountTests(test);
            }

            var count = 0;

            // Check if this test matches the filters
            if (!test.IsSuite)
            {
                bool matches = false;

                // Check assembly filter
                if (!string.IsNullOrEmpty(filterParams.TestAssembly) && test.FullName.Contains(filterParams.TestAssembly))
                {
                    matches = true;
                }

                // Check namespace filter using same regex pattern as Filter.groupNames (ensures sync with Unity's execution)
                if (!matches && !string.IsNullOrEmpty(filterParams.TestNamespace))
                {
                    var namespacePattern = CreateNamespaceRegexPattern(filterParams.TestNamespace);
                    if (System.Text.RegularExpressions.Regex.IsMatch(test.FullName, namespacePattern))
                    {
                        matches = true;
                    }
                }

                // Check class filter using same regex pattern as Filter.groupNames (ensures sync with Unity's execution)
                if (!matches && !string.IsNullOrEmpty(filterParams.TestClass))
                {
                    var classPattern = CreateClassRegexPattern(filterParams.TestClass);
                    if (System.Text.RegularExpressions.Regex.IsMatch(test.FullName, classPattern))
                    {
                        matches = true;
                    }
                }

                // Check method filter (FixtureName.TestName format, same as Filter.testNames)
                if (!matches && !string.IsNullOrEmpty(filterParams.TestMethod))
                {
                    if (test.FullName.Equals(filterParams.TestMethod, StringComparison.OrdinalIgnoreCase))
                    {
                        matches = true;
                    }
                }

                if (matches)
                {
                    count = 1;
                }
            }

            // Recursively check children
            if (test.HasChildren)
            {
                foreach (var child in test.Children)
                {
                    count += CountFilteredTests(child, filterParams);
                }
            }

            return count;
        }

        private async Task<string> RunSequentialTests(TestRunnerApi testRunnerApi, TestFilterParameters filterParams, int timeoutMs, bool runEditMode, bool runPlayMode)
        {
            var combinedCollector = new CombinedTestResultCollector();
            var totalStartTime = DateTime.Now;

            try
            {
                var editModeTestCount = 0;
                var remainingTimeoutMs = timeoutMs;

                // Run EditMode tests if they exist
                if (runEditMode)
                {
                    Debug.Log($"[TestRunner] Starting EditMode tests...");
                    var editModeStartTime = DateTime.Now;
                    var editModeCollector = await RunSingleTestModeWithCollector(TestMode.EditMode, testRunnerApi, filterParams, timeoutMs);
                    combinedCollector.AddResults(editModeCollector);

                    editModeTestCount = editModeCollector.GetSummary().TotalTests;
                    var editModeDuration = DateTime.Now - editModeStartTime;
                    remainingTimeoutMs = Math.Max(1000, timeoutMs - (int)editModeDuration.TotalMilliseconds);

                    Debug.Log($"[TestRunner] EditMode tests completed in {editModeDuration:mm\\:ss\\.fff}.");
                }
                else
                {
                    Debug.Log($"[TestRunner] Skipping EditMode tests (no matching tests found).");
                }

                // Run PlayMode tests if they exist
                if (runPlayMode)
                {
                    Debug.Log($"[TestRunner] Starting PlayMode tests with {remainingTimeoutMs}ms timeout...");
                    var playModeCollector = await RunSingleTestModeWithCollector(TestMode.PlayMode, testRunnerApi, filterParams, remainingTimeoutMs, editModeTestCount);
                    combinedCollector.AddResults(playModeCollector);
                    Debug.Log($"[TestRunner] PlayMode tests completed.");
                }
                else
                {
                    Debug.Log($"[TestRunner] Skipping PlayMode tests (no matching tests found).");
                }

                // Calculate total duration
                var totalDuration = DateTime.Now - totalStartTime;
                combinedCollector.SetTotalDuration(totalDuration);

                // Format combined results - handle case where only one mode ran
                if (runEditMode && runPlayMode)
                {
                    return FormatCombinedResults(combinedCollector);
                }
                else
                {
                    // Only one mode ran, use single mode formatting
                    var collectors = combinedCollector.GetAllCollectors();
                    if (collectors.Any())
                    {
                        return FormatTestResults(collectors.First());
                    }
                    else
                    {
                        return "[Success] No tests were executed (no matching tests found).";
                    }
                }
            }
            catch (Exception ex)
            {
                return Error.TestExecutionFailed($"Sequential test execution failed: {ex.Message}");
            }
        }

        private async Task<TestResultCollector> RunSingleTestModeWithCollector(TestMode testMode, TestRunnerApi testRunnerApi, TestFilterParameters filterParams, int timeoutMs, int previousTestCount = 0)
        {
            var filter = CreateTestFilter(testMode, filterParams);
            var runNumber = testMode == TestMode.EditMode
                ? 1
                : 2;
            var resultCollector = new TestResultCollector(testMode, runNumber);

            await MainThread.Instance.RunAsync(() =>
            {
                testRunnerApi.RegisterCallbacks(resultCollector);
                var executionSettings = new ExecutionSettings(filter);
                testRunnerApi.Execute(executionSettings);
            });

            try
            {
                var timeoutCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMs));
                await resultCollector.WaitForCompletionAsync(timeoutCts.Token);
                return resultCollector;
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning($"[TestRunner] {testMode} tests timed out after {timeoutMs} ms.");
                return resultCollector;
            }
            finally
            {
                await MainThread.Instance.RunAsync(() => testRunnerApi.UnregisterCallbacks(resultCollector));
            }
        }

        private static string FormatTestResults(TestResultCollector collector)
        {
            var results = collector.GetResults();
            var summary = collector.GetSummary();
            var logs = collector.GetLogs();

            var output = new StringBuilder();
            output.AppendLine("[Success] Test execution completed.");
            output.AppendLine();

            // Summary
            output.AppendLine("=== TEST SUMMARY ===");
            output.AppendLine($"Status: {summary.Status}");
            output.AppendLine($"Total: {summary.TotalTests}");
            output.AppendLine($"Passed: {summary.PassedTests}");
            output.AppendLine($"Failed: {summary.FailedTests}");
            output.AppendLine($"Skipped: {summary.SkippedTests}");
            output.AppendLine($"Duration: {summary.Duration:hh\\:mm\\:ss\\.fff}");
            output.AppendLine();

            // Individual test results
            if (results.Any())
            {
                output.AppendLine("=== TEST RESULTS ===");
                foreach (var result in results)
                {
                    output.AppendLine($"[{result.Status}] {result.Name}");
                    output.AppendLine($"  Duration: {result.Duration:ss\\.fff}s");

                    if (!string.IsNullOrEmpty(result.Message))
                    {
                        output.AppendLine($"  Message: {result.Message}");
                    }

                    if (!string.IsNullOrEmpty(result.StackTrace))
                    {
                        output.AppendLine($"  Stack Trace: {result.StackTrace}");
                    }
                    output.AppendLine();
                }
            }

            // Console logs
            if (logs.Any())
            {
                output.AppendLine("=== CONSOLE LOGS ===");
                foreach (var log in logs)
                {
                    output.AppendLine(log);
                }
            }

            return output.ToString();
        }

        private static string FormatCombinedResults(CombinedTestResultCollector collector)
        {
            var results = collector.GetResults();
            var summary = collector.GetSummary();
            var editModeSummary = collector.GetEditModeSummary();
            var playModeSummary = collector.GetPlayModeSummary();
            var logs = collector.GetLogs();

            var output = new StringBuilder();
            output.AppendLine("[Success] Combined test execution completed.");
            output.AppendLine();

            // Combined Summary
            output.AppendLine("=== COMBINED TEST SUMMARY ===");
            var overallStatusColored = summary.Status == TestRunStatus.Passed
                ? "<color=green>✅</color>"
                : "<color=red>❌</color>";
            output.AppendLine($"Overall Status: {summary.Status} {overallStatusColored}");
            output.AppendLine($"Total Tests: {summary.TotalTests}");
            output.AppendLine($"Total Passed: {summary.PassedTests}");
            output.AppendLine($"Total Failed: {summary.FailedTests}");
            output.AppendLine($"Total Skipped: {summary.SkippedTests}");
            output.AppendLine($"Total Duration: {summary.Duration:hh\\:mm\\:ss\\.fff}");
            output.AppendLine();

            // EditMode Summary
            if (editModeSummary.TotalTests > 0)
            {
                var editModeStatusColored = editModeSummary.Status == TestRunStatus.Passed
                    ? "<color=green>✅</color>"
                    : "<color=red>❌</color>";
                output.AppendLine("=== EDITMODE TEST SUMMARY ===");
                output.AppendLine($"Status: {editModeSummary.Status} {editModeStatusColored}");
                output.AppendLine($"Total: {editModeSummary.TotalTests}");
                output.AppendLine($"Passed: {editModeSummary.PassedTests}");
                output.AppendLine($"Failed: {editModeSummary.FailedTests}");
                output.AppendLine($"Skipped: {editModeSummary.SkippedTests}");
                output.AppendLine($"Duration: {editModeSummary.Duration:hh\\:mm\\:ss\\.fff}");
                output.AppendLine();
            }

            // PlayMode Summary
            if (playModeSummary.TotalTests > 0)
            {
                var playModeStatusColored = playModeSummary.Status == TestRunStatus.Passed
                    ? "<color=green>✅</color>"
                    : "<color=red>❌</color>";
                output.AppendLine("=== PLAYMODE TEST SUMMARY ===");
                output.AppendLine($"Status: {playModeSummary.Status} {playModeStatusColored}");
                output.AppendLine($"Total: {playModeSummary.TotalTests}");
                output.AppendLine($"Passed: {playModeSummary.PassedTests}");
                output.AppendLine($"Failed: {playModeSummary.FailedTests}");
                output.AppendLine($"Skipped: {playModeSummary.SkippedTests}");
                output.AppendLine($"Duration: {playModeSummary.Duration:hh\\:mm\\:ss\\.fff}");
                output.AppendLine();
            }

            // Individual test results
            if (results.Any())
            {
                output.AppendLine("=== TEST RESULTS ===");
                foreach (var result in results)
                {
                    output.AppendLine($"[{result.Status}] {result.Name}");
                    output.AppendLine($"  Duration: {result.Duration:ss\\.fff}s");

                    if (!string.IsNullOrEmpty(result.Message))
                    {
                        output.AppendLine($"  Message: {result.Message}");
                    }

                    if (!string.IsNullOrEmpty(result.StackTrace))
                    {
                        output.AppendLine($"  Stack Trace: {result.StackTrace}");
                    }
                    output.AppendLine();
                }
            }

            // Console logs
            if (logs.Any())
            {
                output.AppendLine("=== CONSOLE LOGS ===");
                foreach (var log in logs)
                {
                    output.AppendLine(log);
                }
            }

            return output.ToString();
        }

    }

    // Helper classes for test result collection
    public class TestResultCollector : ICallbacks
    {
        private readonly List<TestResultData> _results = new();
        private readonly List<string> _logs = new();
        private readonly TaskCompletionSource<bool> _completionSource = new();
        private readonly TestSummaryData _summary = new();
        private DateTime _startTime;
        private readonly TestMode _testMode;
        private readonly int _runNumber;

        public TestResultCollector(TestMode testMode, int runNumber = 1)
        {
            _testMode = testMode;
            _runNumber = runNumber;
        }

        public void RunStarted(ITestAdaptor testsToRun)
        {
            _startTime = DateTime.Now;
            var testCount = CountTests(testsToRun);

            _summary.TotalTests = testCount;
            Debug.Log($"[TestRunner] Run {_runNumber} ({_testMode}) started: {testCount} tests.");
        }

        public void RunFinished(ITestResultAdaptor result)
        {
            var endTime = DateTime.Now;
            var duration = endTime - _startTime;
            _summary.Duration = duration;
            if (_summary.FailedTests > 0)
            {
                _summary.Status = TestRunStatus.Failed;
            }
            else if (_summary.PassedTests > 0)
            {
                _summary.Status = TestRunStatus.Passed;
            }
            else
            {
                _summary.Status = TestRunStatus.Unknown;
            }

            Debug.Log($"[TestRunner] Run {_runNumber} ({_testMode}) finished with {CountTests(result.Test)} test results. Result status: {result.TestStatus}");
            Debug.Log($"[TestRunner] Final duration: {duration:mm\\:ss\\.fff}. Completed: {_results.Count}/{_summary.TotalTests}");

            _completionSource.TrySetResult(true);
        }

        public void TestStarted(ITestAdaptor test)
        {
            // Test started - could log this if needed
        }

        public void TestFinished(ITestResultAdaptor result)
        {
            // Only count actual tests, not test suites
            if (!result.Test.IsSuite)
            {
                var testResult = new TestResultData
                {
                    Name = result.Test.FullName,
                    Status = result.TestStatus.ToString(),
                    Duration = TimeSpan.FromSeconds(result.Duration),
                    Message = result.Message,
                    StackTrace = result.StackTrace
                };

                _results.Add(testResult);

                var statusEmoji = result.TestStatus switch
                {
                    TestStatus.Passed => "<color=green>✅</color>",
                    TestStatus.Failed => "<color=red>❌</color>",
                    TestStatus.Skipped => "<color=yellow>⚠️</color>",
                    _ => ""
                };

                Debug.Log($"[TestRunner] {statusEmoji} Test finished: {result.Test.FullName} - {result.TestStatus} ({_results.Count}/{_summary.TotalTests})");

                // Update summary counts
                switch (result.TestStatus)
                {
                    case TestStatus.Passed:
                        _summary.PassedTests++;
                        break;
                    case TestStatus.Failed:
                        _summary.FailedTests++;
                        break;
                    case TestStatus.Skipped:
                        _summary.SkippedTests++;
                        break;
                }

                // Update duration as tests complete
                _summary.Duration = DateTime.Now - _startTime;

                // Check if all tests are complete
                if (_results.Count >= _summary.TotalTests)
                {
                    Debug.Log($"[TestRunner] All tests completed via TestFinished. Final duration: {_summary.Duration:mm\\:ss\\.fff}");
                    _completionSource.TrySetResult(true);
                }
            }
        }

        public async Task WaitForCompletionAsync(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(() => tcs.TrySetCanceled()))
            {
                var completedTask = await Task.WhenAny(_completionSource.Task, tcs.Task);
                if (completedTask == tcs.Task)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
                await _completionSource.Task; // Re-await to get the result or exception
            }
        }

        public List<TestResultData> GetResults() => _results;
        public TestSummaryData GetSummary() => _summary;
        public List<string> GetLogs() => _logs;
        public TestMode GetTestMode() => _testMode;

        public static int CountTests(ITestAdaptor test)
        {
            if (!test.HasChildren)
                return test.IsSuite
                    ? 0
                    : 1;

            return test.Children.Sum(CountTests);
        }
    }

    public class CombinedTestResultCollector
    {
        private readonly List<TestResultData> _allResults = new();
        private readonly List<string> _allLogs = new();
        private readonly List<TestResultCollector> _collectors = new();
        private TestSummaryData _combinedSummary = new();
        private TestSummaryData _editModeSummary = new();
        private TestSummaryData _playModeSummary = new();

        public void AddResults(TestResultCollector collector)
        {
            var results = collector.GetResults();
            var summary = collector.GetSummary();
            var logs = collector.GetLogs();
            var testMode = collector.GetTestMode();

            _collectors.Add(collector);
            _allResults.AddRange(results);
            _allLogs.AddRange(logs);

            _combinedSummary.TotalTests += summary.TotalTests;
            _combinedSummary.PassedTests += summary.PassedTests;
            _combinedSummary.FailedTests += summary.FailedTests;
            _combinedSummary.SkippedTests += summary.SkippedTests;

            if (testMode == TestMode.EditMode)
            {
                _editModeSummary = summary;
            }
            else if (testMode == TestMode.PlayMode)
            {
                _playModeSummary = summary;
            }
        }

        public void SetTotalDuration(TimeSpan duration)
        {
            _combinedSummary.Duration = duration;
            _combinedSummary.Status = _combinedSummary.FailedTests > 0
                ? TestRunStatus.Failed
                : TestRunStatus.Passed;
        }

        public List<TestResultData> GetResults() => _allResults;
        public TestSummaryData GetSummary() => _combinedSummary;
        public TestSummaryData GetEditModeSummary() => _editModeSummary;
        public TestSummaryData GetPlayModeSummary() => _playModeSummary;
        public List<string> GetLogs() => _allLogs;
        public List<TestResultCollector> GetAllCollectors() => _collectors;
    }

    public class TestResultData
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public string? Message { get; set; }
        public string? StackTrace { get; set; }
    }

    public enum TestRunStatus
    {
        Unknown,
        Passed,
        Failed
    }

    public class TestSummaryData
    {
        public TestRunStatus Status { get; set; } = TestRunStatus.Unknown;
        public int TotalTests { get; set; }
        public int PassedTests { get; set; }
        public int FailedTests { get; set; }
        public int SkippedTests { get; set; }
        public TimeSpan Duration { get; set; }
    }
}