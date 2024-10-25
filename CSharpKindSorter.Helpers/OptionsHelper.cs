using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpKindSorter.Helpers;

public static class OptionsHelper
{
	public static List<MemberDeclarationSyntax> GetSortOrder(Options options, List<MemberDeclarationSyntax> orderedMembers)
	{
		return orderedMembers
			.OrderBy(m => GetSortIndex(options, m))
			.ThenBy(m => options.Alphabetical.HasValue && options.Alphabetical.Value ? m.GetName() : string.Empty)
			.ToList();
	}

	private static string GetAccessModifier(MemberDeclarationSyntax member)
	{
		if (member.Modifiers.Any(SyntaxKind.PrivateKeyword))
		{
			return "private";
		}

		if (member is MethodDeclarationSyntax method && method.ExplicitInterfaceSpecifier != null ||
			member is PropertyDeclarationSyntax property && property.ExplicitInterfaceSpecifier != null ||
			member is EventDeclarationSyntax eventDecl && eventDecl.ExplicitInterfaceSpecifier != null)
		{
			return "public explicit";
		}

		if (member.Modifiers.Any(SyntaxKind.ProtectedKeyword) && member.Modifiers.Any(SyntaxKind.InternalKeyword))
		{
			return "protected internal";
		}

		if (member.Modifiers.Any(SyntaxKind.InternalKeyword))
		{
			return "internal";
		}

		if (member.Modifiers.Any(SyntaxKind.ProtectedKeyword))
		{
			return "protected";
		}

		return "public";
	}

	private static string GetKind(MemberDeclarationSyntax member)
	{
		return member switch
		{
			FieldDeclarationSyntax => "Fields",
			ConstructorDeclarationSyntax => "Constructors",
			DestructorDeclarationSyntax => "Finalizers",
			DelegateDeclarationSyntax => "Delegates",
			EventDeclarationSyntax => "Events",
			EventFieldDeclarationSyntax => "Events",
			EnumDeclarationSyntax => "Enums",
			InterfaceDeclarationSyntax => "Interfaces",
			PropertyDeclarationSyntax => "Properties",
			IndexerDeclarationSyntax => "Indexers",
			MethodDeclarationSyntax => "Methods",
			StructDeclarationSyntax => "Structs",
			ClassDeclarationSyntax => "Classes",
			NamespaceDeclarationSyntax => "Namespaces",
			OperatorDeclarationSyntax => "Operators",
			ConversionOperatorDeclarationSyntax => "Operators",
			_ => "Unknown"
		};
	}

	private static int GetSortIndex(Options options, MemberDeclarationSyntax member)
	{
		var memberType = options.KindOrder.ToList().IndexOf(GetKind(member)) * 10000;
		var accessType = options.AccessOrder.ToList().IndexOf(GetAccessModifier(member)) * 1000;

		var total = memberType + accessType;

		if (options.ConstFirst.HasValue && options.ConstFirst.Value)
		{
			total += IsConstant(member) ? 0 : 100;
		}

		if (options.StaticFirst.HasValue && options.StaticFirst.Value)
		{
			total += IsStatic(member) ? 0 : 10;
		}

		if (options.ReadonlyFirst.HasValue && options.ReadonlyFirst.Value)
		{
			total += IsReadonly(member) ? 0 : 1;
		}

		if (options.OverrideFirst.HasValue && options.OverrideFirst.Value)
		{
			total += IsOverride(member) ? 0 : 1;
		}

		return total;
	}

	private static bool IsConstant(MemberDeclarationSyntax member)
	{
		return member.Modifiers.Any(SyntaxKind.ConstKeyword);
	}

	private static bool IsOverride(MemberDeclarationSyntax member)
	{
		return member.Modifiers.Any(SyntaxKind.OverrideKeyword);
	}

	private static bool IsReadonly(MemberDeclarationSyntax member)
	{
		return member.Modifiers.Any(SyntaxKind.ReadOnlyKeyword);
	}

	private static bool IsStatic(MemberDeclarationSyntax member)
	{
		return member.Modifiers.Any(SyntaxKind.StaticKeyword);
	}
}