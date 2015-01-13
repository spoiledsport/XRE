
namespace Microsoft.Framework.Runtime
{
    public class CompilationMessage : ICompilationMessage
    {
        public string Message { get; set; }

        public int EndColumn { get; set; }

        public int EndLine { get; set; }

        public int StartColumn { get; set; }

        public int StartLine { get; set; }
    }
}