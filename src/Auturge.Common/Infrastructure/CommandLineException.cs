using System.CommandLine.Parsing;

namespace Auturge.Common.Infrastructure;

public class CommandLineException(IReadOnlyList<ParseError> parseErrors) : Exception
{
    /// <summary>
    /// Gets the parse errors found while parsing command line input.
    /// </summary>
    public IReadOnlyList<ParseError> ParseErrors { get; } = parseErrors;

    public static CommandLineException From(IReadOnlyList<ParseError> parseErrors) => new(parseErrors);
}
