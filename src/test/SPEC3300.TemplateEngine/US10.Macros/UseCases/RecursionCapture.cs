namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US10.Macros.UseCases;

/// <summary>
/// The observation a recursion case produces: the exception type, the number of macros in the
/// reported chain, and which links the chain contains. Kept beside the use case so the scenario
/// file declares only its own class.
/// </summary>
public readonly record struct RecursionCapture(string Type, int ChainLength, bool HasPhrase, bool HasSelfLoop, bool HasPingPong)
{
    public override string ToString() => $"{Type}:{ChainLength}:{HasPhrase}:{HasSelfLoop}:{HasPingPong}";
}
