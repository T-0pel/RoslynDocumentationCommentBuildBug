using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DocumentationCommentAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DocumentationCommentAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DocumentationCommentAnalyzer";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeType, SyntaxKind.ClassDeclaration);
        }

        private static void AnalyzeType(SyntaxNodeAnalysisContext context)
        {
            var typeDeclaration = (TypeDeclarationSyntax)context.Node;

            // This approach takes care of partial classes
            var symbol = context.SemanticModel.GetDeclaredSymbol(typeDeclaration);

            bool typeHasComment = HasComment(typeDeclaration);

            if (!typeHasComment)
            {
                SimpleLogToFile(typeDeclaration, symbol);
                CreateDiagnostic(typeDeclaration.Identifier.GetLocation());
            }

            void CreateDiagnostic(Location location)
            {
                var diagnostic = Diagnostic.Create(Rule, location);
                context.ReportDiagnostic(diagnostic);
            }

            bool HasComment(MemberDeclarationSyntax member)
            {
                bool hasComment = !string.IsNullOrWhiteSpace(symbol?.GetDocumentationCommentXml());

                return hasComment;
            }
        }

        private static void SimpleLogToFile(TypeDeclarationSyntax typeDeclaration, ISymbol typeSymbol)
        {
            var documentationComment = string.IsNullOrWhiteSpace(typeSymbol.GetDocumentationCommentXml())
                ? "Documentation comment is empty!"
                : typeSymbol.GetDocumentationCommentXml();

            var folderPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\Logs";
            Directory.CreateDirectory(folderPath);
            var filePath = $"{folderPath}\\test.txt";

            File.AppendAllLines(filePath, new List<string>
            {
                $"Identifier: {typeDeclaration.Identifier.Text}",
                "Type declaration full string:",
                typeDeclaration.ToFullString(),
                $"Documentation comment: {documentationComment}",
                "Leading trivia full string:",
                typeDeclaration.GetLeadingTrivia().ToFullString(),
                $"Leading trivia count: {typeDeclaration.GetLeadingTrivia().Count}",
                "Leading trivia kinds:"
            });

            File.AppendAllLines(filePath, typeDeclaration.GetLeadingTrivia().Select(t => t.Kind().ToString()).ToList());
        }
    }
}
