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
        [Description("Execute Unity tests and return detailed results. Supports filtering by test mode, assembly, class, and method.")]
        public async Task<string> Run
        (
            [Description("Test mode to run. Options: 'EditMode', 'PlayMode', 'All'. Default: 'All'")]
            string testMode = "All",
            [Description("Specific test assembly name to run (optional). Example: 'Assembly-CSharp-Editor-testable'")]
            string? testAssembly = null,
            [Description("Specific fully qualified test class name to run (optional). Example: 'MyNamespace.MyTestClass'")]
            string? testClass = null,
            [Description("Specific fully qualified test method name to run (optional). Example: 'MyNamespace.MyTestClass.MyTestMethod'")]
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
                    // Validate both EditMode and PlayMode filters
                    var editModeValidation = await ValidateTestFilters(testRunnerApi, TestMode.EditMode, testAssembly, testClass, testMethod);
                    var playModeValidation = await ValidateTestFilters(testRunnerApi, TestMode.PlayMode, testAssembly, testClass, testMethod);
                    
                    // If both modes failed validation, return error
                    if (editModeValidation != null && playModeValidation != null)
                    {
                        return Error.NoTestsFound(testAssembly, testClass, testMethod);
                    }
                    
                    // Handle "All" mode by running EditMode and PlayMode separately
                    Debug.Log($"[TestRunner] Running ALL tests by executing EditMode and PlayMode sequentially.");
                    return await RunSequentialTests(testRunnerApi, testAssembly, testClass, testMethod, timeoutMs);
                }
                else
                {
                    // Convert string to TestMode enum
                    var testModeEnum = testMode == "EditMode" ? TestMode.EditMode : TestMode.PlayMode;
                    
                    // Validate specific test mode filter
                    var validation = await ValidateTestFilters(testRunnerApi, testModeEnum, testAssembly, testClass, testMethod);
                    if (validation != null)
                        return validation;
                    
                    Debug.Log($"[TestRunner] Running {testMode} tests.");
                    var resultCollector = await RunSingleTestModeWithCollector(testModeEnum, testRunnerApi, testAssembly, testClass, testMethod, timeoutMs);
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

        private static Filter CreateTestFilter(TestMode testMode, string? testAssembly, string? testClass, string? testMethod)
        {
            var filter = new Filter
            {
                // Set test mode
                testMode = testMode
            };

            // Set assembly filter
            if (!string.IsNullOrEmpty(testAssembly))
            {
                filter.assemblyNames = new[] { testAssembly };
            }

            // Set test name filter (class and/or method)
            if (!string.IsNullOrEmpty(testClass) || !string.IsNullOrEmpty(testMethod))
            {
                var testNames = new List<string>();

                if (!string.IsNullOrEmpty(testClass) && !string.IsNullOrEmpty(testMethod))
                {
                    testNames.Add($"{testClass}.{testMethod}");
                }
                else if (!string.IsNullOrEmpty(testClass))
                {
                    testNames.Add(testClass);
                }
                else if (!string.IsNullOrEmpty(testMethod))
                {
                    testNames.Add(testMethod);
                }

                if (testNames.Any())
                    filter.testNames = testNames.ToArray();
            }

            return filter;
        }

        private async Task<string?> ValidateTestFilters(TestRunnerApi testRunnerApi, TestMode testMode, string? testAssembly, string? testClass, string? testMethod)
        {
            try
            {
                var tcs = new TaskCompletionSource<int>();
                
                // Retrieve test list without running tests
                await MainThread.Instance.RunAsync(() =>
                {
                    testRunnerApi.RetrieveTestList(testMode, (testRoot) =>
                    {
                        var testCount = testRoot != null ? CountFilteredTests(testRoot, testAssembly, testClass, testMethod) : 0;
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
                
                var testCount = await tcs.Task;
                
                if (testCount == 0)
                {
                    return Error.NoTestsFound(testAssembly, testClass, testMethod);
                }
                
                return null; // No error, tests found
            }
            catch (OperationCanceledException)
            {
                return Error.NoTestsFound(testAssembly, testClass, testMethod);
            }
            catch (Exception ex)
            {
                return Error.TestExecutionFailed($"Filter validation failed: {ex.Message}");
            }
        }

        private static int CountFilteredTests(ITestAdaptor test, string? testAssembly, string? testClass, string? testMethod)
        {
            // If no filters are specified, count all tests
            if (string.IsNullOrEmpty(testAssembly) && string.IsNullOrEmpty(testClass) && string.IsNullOrEmpty(testMethod))
            {
                return TestResultCollector.CountTests(test);
            }

            var count = 0;

            // Check if this test matches the filters
            if (!test.IsSuite)
            {
                // Check assembly filter - use FullName which may include assembly info
                bool matches = !string.IsNullOrEmpty(testAssembly) && test.FullName.Contains(testAssembly);
                
                // Check class filter
                if (!matches && !string.IsNullOrEmpty(testClass) && test.FullName.Contains(testClass))
                {
                    matches = true;
                }

                // Check method filter  
                if (!matches && !string.IsNullOrEmpty(testMethod) && test.FullName.Contains(testMethod))
                {
                    matches = true;
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
                    count += CountFilteredTests(child, testAssembly, testClass, testMethod);
                }
            }

            return count;
        }

        private async Task<string> RunSequentialTests(TestRunnerApi testRunnerApi, string? testAssembly, string? testClass, string? testMethod, int timeoutMs)
        {
            var combinedCollector = new CombinedTestResultCollector();
            var totalStartTime = DateTime.Now;

            try
            {
                Debug.Log($"[TestRunner] Starting EditMode tests...");
                var editModeStartTime = DateTime.Now;
                var editModeCollector = await RunSingleTestModeWithCollector(TestMode.EditMode, testRunnerApi, testAssembly, testClass, testMethod, timeoutMs);
                combinedCollector.AddResults(editModeCollector);

                var editModeTestCount = editModeCollector.GetSummary().TotalTests;
                var editModeDuration = DateTime.Now - editModeStartTime;
                var remainingTimeoutMs = Math.Max(1000, timeoutMs - (int)editModeDuration.TotalMilliseconds);

                Debug.Log($"[TestRunner] EditMode tests completed in {editModeDuration:mm\\:ss\\.fff}. Starting PlayMode tests with {remainingTimeoutMs}ms timeout...");
                var playModeCollector = await RunSingleTestModeWithCollector(TestMode.PlayMode, testRunnerApi, testAssembly, testClass, testMethod, remainingTimeoutMs, editModeTestCount);
                combinedCollector.AddResults(playModeCollector);

                // Calculate total duration
                var totalDuration = DateTime.Now - totalStartTime;
                combinedCollector.SetTotalDuration(totalDuration);

                // Format combined results
                return FormatCombinedResults(combinedCollector);
            }
            catch (Exception ex)
            {
                return Error.TestExecutionFailed($"Sequential test execution failed: {ex.Message}");
            }
        }

        private async Task<TestResultCollector> RunSingleTestModeWithCollector(TestMode testMode, TestRunnerApi testRunnerApi, string? testAssembly, string? testClass, string? testMethod, int timeoutMs, int previousTestCount = 0)
        {
            var filter = CreateTestFilter(testMode, testAssembly, testClass, testMethod);
            var runNumber = testMode == TestMode.EditMode ? 1 : 2;
            var resultCollector = new TestResultCollector(testMode, runNumber, previousTestCount);

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
            var overallStatusColored = summary.Status == TestRunStatus.Passed ? "<color=green>✅</color>" : "<color=red>❌</color>";
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
                var editModeStatusColored = editModeSummary.Status == TestRunStatus.Passed ? "<color=green>✅</color>" : "<color=red>❌</color>";
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
                var playModeStatusColored = playModeSummary.Status == TestRunStatus.Passed ? "<color=green>✅</color>" : "<color=red>❌</color>";
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
        private int _totalExpectedTests;
        private readonly TestMode _testMode;
        private readonly int _runNumber;
        private readonly int _previousTestCount;

        public TestResultCollector(TestMode testMode, int runNumber = 1, int previousTestCount = 0)
        {
            _testMode = testMode;
            _runNumber = runNumber;
            _previousTestCount = previousTestCount;
        }

        public void RunStarted(ITestAdaptor testsToRun)
        {
            _startTime = DateTime.Now;
            var testCount = CountTests(testsToRun);
            var totalExpected = _previousTestCount + testCount;
            
            _totalExpectedTests = testCount;
            _summary.TotalTests = testCount;
            Debug.Log($"[TestRunner] Run {_runNumber} ({_testMode}) started: {testCount} tests. Total expected: {totalExpected}");

            if (testsToRun.HasChildren)
            {
                var firstFewTests = testsToRun.Children.Take(3).Select(t => t.Name);
                Debug.Log($"[TestRunner] Sample tests in this run: {string.Join(", ", firstFewTests)}");
            }
        }

        public void RunFinished(ITestResultAdaptor result)
        {
            var endTime = DateTime.Now;
            var duration = endTime - _startTime;
            _summary.Duration = duration;
            _summary.Status = _summary.FailedTests > 0 ? TestRunStatus.Failed : (_summary.PassedTests > 0 ? TestRunStatus.Passed : TestRunStatus.Unknown);

            Debug.Log($"[TestRunner] Run {_runNumber} ({_testMode}) finished with {CountTests(result.Test)} test results. Result status: {result.TestStatus}");
            Debug.Log($"[TestRunner] Final duration: {duration:mm\\:ss\\.fff}. Completed: {_results.Count}/{_totalExpectedTests}");

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
                return test.IsSuite ? 0 : 1;

            return test.Children.Sum(CountTests);
        }
    }

    public class CombinedTestResultCollector
    {
        private readonly List<TestResultData> _allResults = new();
        private readonly List<string> _allLogs = new();
        private TestSummaryData _combinedSummary = new();
        private TestSummaryData _editModeSummary = new();
        private TestSummaryData _playModeSummary = new();

        public void AddResults(TestResultCollector collector)
        {
            var results = collector.GetResults();
            var summary = collector.GetSummary();
            var logs = collector.GetLogs();
            var testMode = collector.GetTestMode();

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
            _combinedSummary.Status = _combinedSummary.FailedTests > 0 ? TestRunStatus.Failed : TestRunStatus.Passed;
        }

        public List<TestResultData> GetResults() => _allResults;
        public TestSummaryData GetSummary() => _combinedSummary;
        public TestSummaryData GetEditModeSummary() => _editModeSummary;
        public TestSummaryData GetPlayModeSummary() => _playModeSummary;
        public List<string> GetLogs() => _allLogs;
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