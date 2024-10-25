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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CSKS001CodeFixProvider)), Shared]
public class CSKS001CodeFixProvider : CodeFixProvider
{
	public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(CSKS001Analyzer.DiagnosticId);

	public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		var diagnostic = context.Diagnostics.First();
		var diagnosticSpan = diagnostic.Location.SourceSpan;

		var typeDeclarationSyntax = root.FindNode(diagnosticSpan).AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();

		if (diagnostic.Properties.TryGetValue(CSKS001Analyzer.ConfigPropertyKey, out var configJson))
		{
			var orderOptions = Options.GetOptions(configJson);

			if (orderOptions != null)
			{
				context.RegisterCodeFix(
					CodeAction.Create(
						title: "CSKS001 - Fix code sort order.",
						createChangedDocument: c => FixSortOrderAsync(context.Document, typeDeclarationSyntax, orderOptions, c),
						equivalenceKey: nameof(CSKS001CodeFixProvider)),
					diagnostic);
			}
		}
	}

	private static async Task<Document> FixSortOrderAsync(Document document, TypeDeclarationSyntax typeDeclarationSyntax, Options options, CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

		var sortedMembers = Order(typeDeclarationSyntax, options);

		var newRoot = root.ReplaceNode(typeDeclarationSyntax, sortedMembers);

		var newDocument = document.WithSyntaxRoot(newRoot);

		var documentOptions = await newDocument.GetOptionsAsync(cancellationToken);
		var formattedDocument = await Formatter.FormatAsync(newDocument, documentOptions, cancellationToken);

		return formattedDocument;
	}

	private static TypeDeclarationSyntax Order(TypeDeclarationSyntax typeDeclaration, Options options)
	{
		// Recursively order all nested type declarations first
		var orderedMembers = typeDeclaration.Members
			.Select(member =>
			{
				// If the member is a type declaration (class, struct, or interface), process it recursively
				if (member is TypeDeclarationSyntax nestedType)
				{
					return Order(nestedType, options);
				}

				return member;
			})
			.ToList();

		var orderedKinds = OptionsHelper.GetSortOrder(options, orderedMembers);

		var newNamespace = typeDeclaration
			.WithMembers(SyntaxFactory.List(orderedKinds))
			.WithLeadingTrivia(typeDeclaration.GetLeadingTrivia())
			.WithTrailingTrivia(typeDeclaration.GetTrailingTrivia())
			.WithAdditionalAnnotations(Formatter.Annotation);

		return newNamespace;
	}
}