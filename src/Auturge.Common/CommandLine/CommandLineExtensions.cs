using System.CommandLine;
using Auturge.Common.Infrastructure;

namespace Auturge.Common.CommandLine;

public static class CommandLineExtensions
{
    /// <summary>
    /// Gets an exception wrapping any command-line parse errors, if applicable.
    /// </summary>
    /// <param name="parseResult">The result of parsing the command-line arguments.</param>
    /// <returns>An exception, if there are any errors; otherwise, <see langword="null"/>.</returns>
    public static CommandLineException? GetException(this ParseResult parseResult)
        => parseResult.Errors.Count != 0 ? CommandLineException.From(parseResult.Errors) : null;
}
