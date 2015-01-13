using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Framework.Runtime
{
    public struct DiagnosticResult : IDiagnosticResult
    {
        public static readonly DiagnosticResult Successful = new DiagnosticResult(success: true, 
                                                                                  warnings: Enumerable.Empty<string>(), 
                                                                                  errors: Enumerable.Empty<ICompilationFailure>());

        private readonly bool _success;
        private readonly IEnumerable<string> _warnings;
        private readonly IEnumerable<ICompilationFailure> _errors;

        public DiagnosticResult(bool success, IEnumerable<string> warnings, IEnumerable<ICompilationFailure> errors)
        {
            _success = success;
            _warnings = warnings;
            _errors = errors;
        }

        public bool Success
        {
            get
            {
                return _success;
            }
        }

        public IEnumerable<string> Warnings
        {
            get
            {
                return _warnings;
            }
        }

        public IEnumerable<ICompilationFailure> Errors
        {
            get
            {
                return _errors;
            }
        }
    }
}