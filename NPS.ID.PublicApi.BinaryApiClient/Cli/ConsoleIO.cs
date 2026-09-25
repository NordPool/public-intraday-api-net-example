using System.Text;

namespace NPS.ID.PublicApi.BinaryApiClient.Cli;

/// <summary>
/// Synchronised console I/O that keeps the user's input prompt visually
/// separated from asynchronous output.  Output lines are printed above the
/// prompt, and the prompt is redrawn with whatever the user has typed so far.
/// </summary>
public static class ConsoleIO
{
    private const string Prompt = "> ";

    private static readonly Lock Lock = new();
    private static readonly StringBuilder InputBuffer = new();
    private static bool _promptVisible;

    /// <summary>
    /// Writes one or more lines to the console, preserving the current
    /// input prompt.  Thread-safe — can be called from any async context.
    /// </summary>
    public static void Write(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        lock (Lock)
        {
            ClearPromptLine();
            Console.WriteLine(text);
            RedrawPrompt();
        }
    }

    /// <summary>Same as <see cref="Write"/> but rendered in red.</summary>
    public static void WriteError(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        lock (Lock)
        {
            ClearPromptLine();
            var previous = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(text);
            Console.ForegroundColor = previous;
            RedrawPrompt();
        }
    }

    /// <summary>
    /// Reads a full line of input character-by-character so that
    /// concurrent <see cref="Write"/> calls can safely redraw the prompt.
    /// Returns null when stdin is closed or cancellation is requested.
    /// </summary>
    public static async Task<string?> ReadLineAsync(CancellationToken ct)
    {
        lock (Lock)
        {
            InputBuffer.Clear();
            RedrawPrompt();
        }

        while (!ct.IsCancellationRequested)
        {
            // Yield until a key is available — avoids busy-waiting.
            while (!Console.KeyAvailable)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Delay(50, ct);
            }

            ConsoleKeyInfo key;
            try
            {
                key = Console.ReadKey(intercept: true);
            }
            catch (InvalidOperationException)
            {
                return null; // stdin redirected / closed
            }

            lock (Lock)
            {
                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                    {
                        var line = InputBuffer.ToString();
                        InputBuffer.Clear();
                        // Move past the prompt line so the next output starts fresh.
                        Console.WriteLine();
                        _promptVisible = false;
                        return line;
                    }

                    case ConsoleKey.Backspace:
                    {
                        if (InputBuffer.Length > 0)
                        {
                            InputBuffer.Remove(InputBuffer.Length - 1, 1);
                            RedrawPrompt();
                        }

                        break;
                    }

                    case ConsoleKey.Escape:
                    {
                        InputBuffer.Clear();
                        RedrawPrompt();
                        break;
                    }

                    default:
                    {
                        if (key.KeyChar >= ' ') // printable
                        {
                            InputBuffer.Append(key.KeyChar);
                            RedrawPrompt();
                        }

                        break;
                    }
                }
            }
        }

        return null;
    }

    // ── internals (caller must hold Lock) ────────────────────────────────

    private static void ClearPromptLine()
    {
        if (!_promptVisible) return;
        // \x1b[2K  = ANSI: erase entire line
        // \r       = carriage return (cursor to column 0)
        Console.Write("\x1b[2K\r");
        _promptVisible = false;
    }

    private static void RedrawPrompt()
    {
        ClearPromptLine();
        Console.Write(Prompt);
        Console.Write(InputBuffer);
        _promptVisible = true;
    }
}
