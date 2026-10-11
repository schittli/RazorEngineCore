using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace RazorEngineCore
{
    public class RazorEngineCompilationException : RazorEngineException
    {
        public required List<Diagnostic> Errors { get; set; }
        
        public required string GeneratedCode { get; set; }

        public override string Message
        {
            get
            {
                var errors = string.Join("\n", Errors.Where(w => w.IsWarningAsError || w.Severity == DiagnosticSeverity.Error));

                // Exception with unqiue ID
                return "2qdilo Unable to compile template: " + errors;
            }
        }
    }
}