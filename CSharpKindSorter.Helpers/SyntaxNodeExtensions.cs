using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpKindSorter.Helpers;

public static class SyntaxNodeExtensions
{
	public static string GetName(this MemberDeclarationSyntax member)
	{
		return member switch
		{
			FieldDeclarationSyntax field => string.Join(",", field.Declaration.Variables.Select(v => v.Identifier.Text)),
			ConstructorDeclarationSyntax constructor => constructor.Identifier.Text,
			DestructorDeclarationSyntax destructor => destructor.Identifier.Text,
			DelegateDeclarationSyntax del => del.Identifier.Text,
			EventDeclarationSyntax evt => evt.Identifier.Text,
			EnumDeclarationSyntax enm => enm.Identifier.Text,
			InterfaceDeclarationSyntax intf => intf.Identifier.Text,
			PropertyDeclarationSyntax prop => prop.Identifier.Text,
			IndexerDeclarationSyntax idx => idx.ThisKeyword.Text,
			MethodDeclarationSyntax method => method.Identifier.Text,
			StructDeclarationSyntax strct => strct.Identifier.Text,
			ClassDeclarationSyntax cls => cls.Identifier.Text,
			_ => string.Empty
		};
	}
}