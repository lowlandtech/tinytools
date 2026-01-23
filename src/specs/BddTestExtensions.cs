using System.Runtime.CompilerServices;

namespace LowlandTech.Specs;

/// <summary>
/// Extension methods for BDD test auto-execution and validation.
/// </summary>
public static class BddTestExtensions
{
    /// <summary>
    /// Auto-validates the current test method's [Then] attribute assertions.
    /// </summary>
    /// <param name="testInstance">The test instance.</param>
    /// <param name="callerMemberName">Auto-populated by compiler.</param>
    public static void AutoValidate(this object testInstance, [CallerMemberName] string callerMemberName = "")
    {
        BddAutoExecutor.AutoValidate(testInstance, callerMemberName);
    }

    /// <summary>
    /// Auto-applies all [Given] preconditions from attributes to the state object.
    /// </summary>
    public static void AutoApplyGivens(this object testInstance)
    {
        BddAutoExecutor.AutoApplyGivens(testInstance);
    }
}
