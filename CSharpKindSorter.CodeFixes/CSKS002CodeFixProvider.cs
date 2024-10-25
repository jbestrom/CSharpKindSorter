using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpKindSorter.Analyzers;
using CSharpKindSorter.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace CSharpKindSorter;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CSKS002CodeFixProvider)), Shared]
public class CSKS002CodeFixProvider : CodeFixProvider
{
	public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(CSKS002Analyzer.DiagnosticId);

	public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		var diagnostic = context.Diagnostics.First();
		var diagnosticSpan = diagnostic.Location.SourceSpan;

		var namespaceDeclaration = root.FindNode(diagnosticSpan).AncestorsAndSelf().OfType<NamespaceDeclarationSyntax>().First();

		if (diagnostic.Properties.TryGetValue(CSKS002Analyzer.ConfigPropertyKey, out var configJson))
		{
			var orderOptions = Options.GetOptions(configJson);

			if (orderOptions != null)
			{
				context.RegisterCodeFix(
					CodeAction.Create(
						title: "CSKS002 - Fix code sort order.",
						createChangedDocument: c => FixSortOrderAsync(context.Document, namespaceDeclaration, orderOptions, c),
						equivalenceKey: nameof(CSKS002CodeFixProvider)),
					diagnostic);
			}
		}
	}

	private static async Task<Document> FixSortOrderAsync(Document document, NamespaceDeclarationSyntax namespaceDeclaration, Options options, CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

		var sortedMembers = Order(namespaceDeclaration, options);

		var newRoot = root.ReplaceNode(namespaceDeclaration, sortedMembers);

		var newDocument = document.WithSyntaxRoot(newRoot);

		var documentOptions = await newDocument.GetOptionsAsync(cancellationToken);
		var formattedDocument = await Formatter.FormatAsync(newDocument, documentOptions, cancellationToken);

		return formattedDocument;
	}

	private static NamespaceDeclarationSyntax Order(NamespaceDeclarationSyntax namespaceDeclaration, Options options)
	{
		var orderedKinds = OptionsHelper.GetSortOrder(options, namespaceDeclaration.Members.ToList());

		var newNamespace = namespaceDeclaration
			.WithMembers(SyntaxFactory.List(orderedKinds))
			.WithLeadingTrivia(namespaceDeclaration.GetLeadingTrivia())
			.WithTrailingTrivia(namespaceDeclaration.GetTrailingTrivia())
			.WithAdditionalAnnotations(Formatter.Annotation);

		return newNamespace;
	}
}