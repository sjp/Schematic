using System;
using System.Runtime.InteropServices;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace SJP.Schematic.Tests.Utilities
{
    /// <summary>
    /// Contains attributes used to filter tests by platform.
    /// </summary>
    public static class TestPlatform
    {
        /// <summary>
        /// When applied to a test, ensures it is only run on a Windows environment.
        /// </summary>
        /// <seealso cref="NUnitAttribute" />
        /// <seealso cref="IApplyToTest" />
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
        public sealed class WindowsAttribute : NUnitAttribute, IApplyToTest
        {
            /// <summary>
            /// Modifies a test as defined for the specific attribute.
            /// </summary>
            /// <param name="test">The test to modify</param>
            public void ApplyToTest(Test test)
            {
                if (test.RunState == RunState.NotRunnable || _isWindows)
                    return;

                test.RunState = RunState.Ignored;

                const string reason = "This test is ignored because the current platform is non-Windows and the test is for Windows platforms only.";
                test.Properties.Set(PropertyNames.SkipReason, reason);
            }

            private readonly static bool _isWindows = RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
        }
    }
}
