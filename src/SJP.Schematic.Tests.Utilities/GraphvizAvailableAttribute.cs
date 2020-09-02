﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace SJP.Schematic.Tests.Utilities
{
    /// <summary>
    /// When applied to a test, ensures it is only run on a Windows environment.
    /// </summary>
    /// <seealso cref="NUnitAttribute" />
    /// <seealso cref="IApplyToTest" />
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class GraphvizAvailableAttribute : NUnitAttribute, IApplyToTest
    {
        /// <summary>
        /// Modifies a test as defined for the specific attribute.
        /// </summary>
        /// <param name="test">The test to modify</param>
        public void ApplyToTest(Test test)
        {
            if (test.RunState == RunState.NotRunnable || IsWindows || HasSystemDot)
                return;

            test.RunState = RunState.Ignored;

            const string reason = "This test is ignored because the test environment does not have a graphviz installed.";
            test.Properties.Set(PropertyNames.SkipReason, reason);
        }

        private static bool IsWindows { get; } = RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);

        private static bool HasSystemDot => _hasSystemDot.Value;

        private static readonly Lazy<bool> _hasSystemDot = new Lazy<bool>(SystemDotExists);

        private static bool SystemDotExists()
        {
            var startInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                FileName = "dot",
                WindowStyle = ProcessWindowStyle.Hidden
            };

            try
            {
                using var process = new Process { StartInfo = startInfo };
                process.Start();
                process.WaitForExit();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
