
using System.Reactive.Linq;
using System.Diagnostics;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.Formatting;

RunAsync().Wait();

async Task RunAsync()
{
    try
    {
        var kernel = new CSharpKernel();
        using (IDisposable subscription = kernel.KernelEvents.Subscribe(e => OnEvent(e)))
        {
            //KernelCommandResult? result = await kernel.SubmitCodeAsync("System.Console.WriteLine(\"foobar\");");
            //KernelCommandResult? result = await kernel.SubmitCodeAsync("int a = 0;");
            KernelCommandResult? result = await kernel.SubmitCodeAsync("int a = 10;");
            await kernel.SubmitCodeAsync("a");
        }
        //result = await kernel.SubmitCodeAsync("a.");
        var position = new LinePosition(0, 2);
        var command = new RequestCompletions("a.", position);
        var context = KernelInvocationContext.Establish(command);

        CompletionsProduced? completionsProduced;
        using (IDisposable subscription2 = context.KernelEvents.Subscribe(e => OnEvent(e)))
        {
            await kernel.HandleAsync(command, context);

            completionsProduced = await context
                                           .KernelEvents
                                           .OfType<CompletionsProduced>()
                                           .FirstOrDefaultAsync();

            // Avoid doing Console.WriteLines whilst the context is not disposed

        }
        if (completionsProduced != null)
        {
            foreach (var item in completionsProduced.Completions)
            {
                Console.WriteLine(item.DisplayText);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
    }
}

void OnEvent(KernelEvent e)
{
    string prefix = ">>>> ";
    switch (e)
    {
        case IncompleteCodeSubmissionReceived incompleteCodeReceived:
            Debug.WriteLine(prefix + e);
            break;

        case CompleteCodeSubmissionReceived completeCodeReceived:
            Debug.WriteLine(prefix + e);
            break;

        case CodeSubmissionReceived codeSubmissionReceived:
            Debug.WriteLine(prefix + e);
            break;

        case DiagnosticsProduced diagnosticsProduced:
            // Errors will be reported in CommandFailed.
            //foreach (FormattedValue? diagnostic in diagnosticsProduced.FormattedDiagnostics)
            //{
            //    Debug.WriteLine(prefix + diagnostic.Value);
            //}
            break;
        case CommandFailed:
            Debug.WriteLine(prefix + e);
            break;
        case CommandSucceeded:
            Debug.WriteLine(prefix + e);
            break;
        case StandardOutputValueProduced standardOutputValueProduced:
            foreach (FormattedValue? value in standardOutputValueProduced.FormattedValues)
            {
                Debug.Write(prefix + value.Value);
            }
            break;
        case StandardErrorValueProduced standardErrorValueProduced:
            foreach (FormattedValue? value in standardErrorValueProduced.FormattedValues)
            {
                Debug.Write(prefix + value.Value);
            }
            break;
        case ReturnValueProduced returnValueProduced:
            foreach (FormattedValue? value in returnValueProduced.FormattedValues)
            {
                Debug.WriteLine(prefix + value.Value);
            }
            break;
        default:
            Debug.WriteLine(prefix + e);
            break;
    }
}