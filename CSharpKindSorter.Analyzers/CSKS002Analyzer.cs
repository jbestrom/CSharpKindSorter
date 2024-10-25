using System.Collections.Immutable;
using System.Linq;
using CSharpKindSorter.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpKindSorter.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CSKS002Analyzer : DiagnosticAnalyzer
{
	public const string ConfigPropertyKey = "SortOptions";
	public const string DiagnosticId = "CSKS002";

	private static DiagnosticDescriptor Rule = new(
		DiagnosticId,
		"Kind sort order",
		"{0} is not sorted correctly",
		"CSharpKindSorter.SortingRules",
		DiagnosticSeverity.Warning,
		true,
		"Kinds should be sorted correctly.", "https://github.com/jbestrom/CSharpKindSorter/tree/master/Documentation/CSKS002.md");

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterCompilationStartAction(AnalyzeCompilationStart);
	}

	private static void AnalyzeCompilationStart(CompilationStartAnalysisContext context)
	{
		var sortOptions = Options.GetOptions(context.Options.AdditionalFiles);
		if (sortOptions != null)
		{
			context.RegisterSyntaxNodeAction(c => AnalyzeSortOrder(c, sortOptions), SyntaxKind.NamespaceDeclaration);
		}
	}

	private static void AnalyzeSortOrder(SyntaxNodeAnalysisContext context, Options options)
	{
		var namespaceDeclaration = (NamespaceDeclarationSyntax)context.Node;

		var kinds = namespaceDeclaration.Members.ToList();

		var sortedKinds = OptionsHelper.GetSortOrder(options, kinds);

		if (!kinds.SequenceEqual(sortedKinds))
		{
			var diagnostic = Diagnostic.Create(
				Rule,
				namespaceDeclaration.Name.GetLocation(),
				ImmutableDictionary.Create<string, string>().Add(ConfigPropertyKey, Options.SerializeOptions(options)),
				namespaceDeclaration.Name.ToString());
			context.ReportDiagnostic(diagnostic);
		}
	}
}