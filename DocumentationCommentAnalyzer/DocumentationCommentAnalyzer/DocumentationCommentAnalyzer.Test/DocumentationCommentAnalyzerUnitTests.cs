using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = DocumentationCommentAnalyzer.Test.CSharpAnalyzerVerifier<DocumentationCommentAnalyzer.DocumentationCommentAnalyzerAnalyzer>;

namespace DocumentationCommentAnalyzer.Test
{
    [TestClass]
    public class DocumentationCommentAnalyzerUnitTest
    {
        [TestMethod]
        public async Task NoDocumentationComment_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

namespace MyNamespace
{
    internal class [|OuterClass|] { }
}
");
        }

        [TestMethod]
        public async Task DocumentationComment_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

namespace MyNamespace
{
    /// <summary>
    /// 
    /// </summary>
    internal class OuterClass { }
}
");
        }
    }
}
