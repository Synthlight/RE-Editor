#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RE_Editor.Common;
using RE_Editor.Common.Data;
using RE_Editor.Common.Models;

namespace RE_Editor.Windows;

public partial class RunCodeWindow {
    private readonly ReDataFile                       file;
    private          CompletionWindow?                completionWindow;
    public           List<string>                     TargetCompletionEntries { get; set; } = [];
    public           Dictionary<string, List<string>> GeneratedTypes          { get; }      = [];

    public RunCodeWindow(Window window, ReDataFile file) {
        this.file = file;

        // TODO: Change fields so we can show the field type to the user.
        foreach (var structInfo in from structInfo in DataHelper.STRUCT_INFO.Values
                                   orderby structInfo.name
                                   select structInfo) {
            var name = structInfo.name?.ToConvertedTypeName()!;
            var fields = from field in structInfo.fields
                         select field.name.ToConvertedFieldName();
            GeneratedTypes[name] = fields.ToList();
        }

        InitializeComponent();

        Owner = window;
        main_text.Text = """
                         This is a very barebones way to run code on a file, but it does work.

                         There's two options here, you can either just implement the function contents, and let this wrap it in the boilerplate code as it runs.
                         That works most of the time, `using` directives are setup for most things. If you need more control over something like that, you can always provide a full complete file and this will compile from that without wrapping in anything.
                         (It'll wrap so long as the text doesn't start with `using`, or `namespace`.)

                         Use the example button to be taken to the wiki page with examples.
                         Use the API button to be taken to nothing :P (The API docs for the classes aren't created yet and this is not 1:1 with the RSZ. (Though, usually, it's just some typo-fixing of the field names and removing of the preceding underscore.))

                         Code completion *does* work... kinda. It will pop up when you type `.` and will 'insert' on any non-letter/digit.
                         The 'kinda' part... Completion does not know a thing about what's in the text box. You need to manually set the desired type.
                         It's also only ever told about game struct types. You won't find anything else in the type list.
                         (Opening the target drop-down-list can take a while, there's 20k+ entries in it.
                         It is setup as a filtered dropdown (min of 5 chars required before it filters) for performance. e.g type 'ItemData' (not case sensitive) in it and the list will have very few entries.)
                         """;

        code_box.TextArea.TextEntering += TextArea_TextEntering;
        code_box.TextArea.TextEntered  += TextArea_TextEntered;
    }

    private void TextArea_TextEntered(object sender, TextCompositionEventArgs e) {
        if (e.Text == ".") {
            // Open code completion after the user has pressed dot:
            completionWindow = new(code_box.TextArea);
            var data = completionWindow.CompletionList.CompletionData;
            foreach (var entry in TargetCompletionEntries) {
                data.Add(new CompletionBase(entry));
            }
            completionWindow.Show();
            completionWindow.Closed += delegate { completionWindow = null; };
        }
    }

    private void TextArea_TextEntering(object sender, TextCompositionEventArgs e) {
        if (e.Text.Length > 0 && completionWindow != null) {
            if (!char.IsLetterOrDigit(e.Text[0])) {
                // Whenever a non-letter is typed while the completion window is open,
                // insert the currently selected element.
                completionWindow.CompletionList.RequestInsertion(e);
            }
        }
        // Do not set `e.Handled` to true.
        // We still want to insert the character that was typed.
    }

    private void Btn_open_Click(object sender, RoutedEventArgs e) {
    }

    private void Btn_save_Click(object sender, RoutedEventArgs e) {
    }

    private void Btn_save_as_Click(object sender, RoutedEventArgs e) {
    }

    private void Btn_open_examples_OnClick(object sender, RoutedEventArgs e) {
        try {
            Process.Start(new ProcessStartInfo("cmd", "/c start https://github.com/Synthlight/RE-Editor/wiki/Running-Code-on-Files") {CreateNoWindow = true});
        } catch (Exception err) {
            MainWindow.ShowError(err, "Error Opening Examples");
        }
    }

    private void Btn_open_api_OnClick(object sender, RoutedEventArgs e) {
        try {
            //Process.Start(new ProcessStartInfo("cmd", $"/c start {PathHelper.WIKI_URL}") {CreateNoWindow = true});
        } catch (Exception err) {
            MainWindow.ShowError(err, "Error Opening API Docs");
        }
    }

    private void Btn_run_code_Click(object sender, RoutedEventArgs e) {
        var realCodeStartOffset = 0;
        var code                = code_box.Text;
        if (!code.StartsWith("using") && !code.StartsWith("namespace")) {
            code = $$"""
                     using System;
                     using System.Collections.Generic;
                     using System.Diagnostics;
                     using System.IO;
                     using System.Linq;
                     using RE_Editor.Common;
                     using RE_Editor.Common.Data;
                     using RE_Editor.Common.Models;
                     using RE_Editor.Constants;
                     using RE_Editor.Models;
                     using RE_Editor.Models.Enums;
                     using RE_Editor.Models.Structs;
                     using RE_Editor.Util;
                     using RE_Editor.Windows;

                     namespace RE_Editor.Mods;

                     public static class RuntimeEdits {
                         public static void DoStuff(ReDataFile file) {
                     {{code}}
                         }
                     }
                     """;
            realCodeStartOffset = 19;
        }

        var parseOptions = new CSharpParseOptions(LanguageVersion.Latest);
        var syntaxTree   = CSharpSyntaxTree.ParseText(code, parseOptions);
        var metadataReferences = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                 where !string.IsNullOrWhiteSpace(assembly.Location)
                                 select MetadataReference.CreateFromFile(assembly.Location);
        var       compOptions  = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
        var       compilation  = CSharpCompilation.Create(null, [syntaxTree], metadataReferences, compOptions);
        using var memoryStream = new MemoryStream();
        var       result       = compilation.Emit(memoryStream);

        if (result.Success) {
            try {
                memoryStream.Seek(0, SeekOrigin.Begin);
                var loadContext = new AssemblyLoadContext("TempRuntimeEditContext", true);
                var assembly    = loadContext.LoadFromStream(memoryStream);
                var methodInfo  = assembly.GetType("RE_Editor.Mods.RuntimeEdits")?.GetMethod("DoStuff");
                if (methodInfo == null) {
                    MessageBox.Show("Unable to find the entry type/method: RE_Editor.Mods.RuntimeEdits.DoStuff", "Errors Running Code", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                methodInfo.Invoke(null, [file]);
                loadContext.Unload();
                MessageBox.Show("Code Ran Successfully :)", "Code Ran Successfully", MessageBoxButton.OK, MessageBoxImage.Information);
            } catch (Exception err) {
                MessageBox.Show(err.ToString(), "Errors Running Code", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        } else {
            var failures = result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);
            var msg      = new StringBuilder();

            foreach (var diagnostic in failures) {
                var message = diagnostic.GetMessage();
                var pos     = diagnostic.Location.GetLineSpan();
                var line    = $"(@{(pos.StartLinePosition.Line + 1 - realCodeStartOffset)}:{(pos.StartLinePosition.Character + 1)})";
                msg.Append($"{diagnostic.Id}: {message} {line}\n");
            }

            MessageBox.Show(msg.ToString(), "Errors Compiling Code", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private class CompletionBase(string text) : ICompletionData {
        public ImageSource? Image       => null;
        public string       Text        { get; } = text;
        public object       Content     => Text;
        public object?      Description => null;
        public double       Priority    => 1;

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs) {
            textArea.Document.Replace(completionSegment, Text);
        }
    }
}