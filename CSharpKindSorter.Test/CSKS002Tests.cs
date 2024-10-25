using System.Threading.Tasks;
using CSharpKindSorter.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpKindSorter.Test;

[TestClass]
public class CSKS002Tests : CSharpCodeFixVerifier<CSKS002Analyzer, CSKS002CodeFixProvider, DefaultVerifier>
{
	private const string DEFAULT_INPUT_CODE = @"
namespace TestNamespace
{
    // Class Comment
    public class TestClassC
    {
    }

    // Struct Comment
    public struct TestStruct
    {
    }

    // Interface Comment
    public interface ITestInterface
    {
    } // After Interface Comment

    // Enum Comment
    public enum TestEnum2
    {
        FirstValue,
        SecondValue
    }

    /*
       This is a multiline
       comment
    */
    public enum TestEnum1
    {
        FirstValue,
        SecondValue
    }

    // Delegate Comment
    public delegate void TestDelegate(int value);
    public delegate void TestDelegate2(int value);

    // Class Comment A
    // Class Comment B
    public class TestClassA
    {
    }

    internal class TestClassB
    {
    }

    // Interface Comment2
    public interface ITestInterface2
    {
    }

    // Delegate Comment 3
    public delegate void TestDelegate3(int value);
    internal delegate void TestDelegateA(int value);
}";

	[TestMethod]
	public async Task CustomJson001()
	{
		var expectedFixedCode = @"
namespace TestNamespace
{
    internal delegate void TestDelegateA(int value);

    // Delegate Comment
    public delegate void TestDelegate(int value);
    public delegate void TestDelegate2(int value);

    // Delegate Comment 3
    public delegate void TestDelegate3(int value);

    /*
       This is a multiline
       comment
    */
    public enum TestEnum1
    {
        FirstValue,
        SecondValue
    }

    // Enum Comment
    public enum TestEnum2
    {
        FirstValue,
        SecondValue
    }

    // Interface Comment
    public interface ITestInterface
    {
    } // After Interface Comment

    // Interface Comment2
    public interface ITestInterface2
    {
    }

    // Struct Comment
    public struct TestStruct
    {
    }

    internal class TestClassB
    {
    }

    // Class Comment A
    // Class Comment B
    public class TestClassA
    {
    }
    // Class Comment
    public class TestClassC
    {
    }
}";

		await new CSharpCodeFixTest<CSKS002Analyzer, CSKS002CodeFixProvider, DefaultVerifier>
		{
			TestState =
			{
				Sources = { DEFAULT_INPUT_CODE }
				,AnalyzerConfigFiles = { ("/.editorconfig", @"
root = true

[*]
# Standard properties
end_of_line = lf
trim_trailing_whitespace = true
insert_final_newline = false
indent_size = 4
tab_width = 4
") }

				,
				AdditionalFiles =
				{
					("csharpkindsorter.json", "{\r\n  \"KindOrder\": [ \"ConstantFields\", \"Fields\", \"Constructors\", \"Finalizers\", \"Delegates\", \"Events\", \"Enums\", \"Interfaces\", \"Properties\", \"Indexers\", \"Methods\", \"Structs\", \"Classes\" ],\r\n  \"AccessOrder\": [ \"internal\", \"public\", \"protected internal\", \"protected\", \"private\" ],\r\n  \"StaticFirst\": true,\r\n  \"ReadonlyFirst\": true,\r\n  \"Alphabetical\": true\r\n}")
				}
			},
			FixedState =
			{
				Sources = { expectedFixedCode }
			},
			ExpectedDiagnostics = { new DiagnosticResult(CSKS002Analyzer.DiagnosticId, DiagnosticSeverity.Warning)
				.WithSpan(2, 11, 2, 24)
				.WithArguments("TestNamespace") }
		}.RunAsync();
	}

	[TestMethod]
	public async Task CustomJson002()
	{
		var expectedFixedCode = @"
namespace TestNamespace
{

    // Delegate Comment
    public delegate void TestDelegate(int value);
    public delegate void TestDelegate2(int value);

    // Delegate Comment 3
    public delegate void TestDelegate3(int value);
    internal delegate void TestDelegateA(int value);

    // Class Comment A
    // Class Comment B
    public class TestClassA
    {
    }
    // Class Comment
    public class TestClassC
    {
    }

    internal class TestClassB
    {
    }

    /*
       This is a multiline
       comment
    */
    public enum TestEnum1
    {
        FirstValue,
        SecondValue
    }

    // Enum Comment
    public enum TestEnum2
    {
        FirstValue,
        SecondValue
    }

    // Struct Comment
    public struct TestStruct
    {
    }

    // Interface Comment
    public interface ITestInterface
    {
    } // After Interface Comment

    // Interface Comment2
    public interface ITestInterface2
    {
    }
}";

		await new CSharpCodeFixTest<CSKS002Analyzer,
			CSKS002CodeFixProvider, DefaultVerifier>
		{
			TestState =
			{
				Sources = { DEFAULT_INPUT_CODE }
				,AnalyzerConfigFiles = { ("/.editorconfig", @"
root = true

[*]
# Standard properties
end_of_line = lf
trim_trailing_whitespace = true
insert_final_newline = false
indent_size = 4
tab_width = 4
") }
				,
				AdditionalFiles =
				{
					("csharpkindsorter.json", "{\r\n  \"KindOrder\": [ \"ConstantFields\", \"Fields\", \"Constructors\", \"Finalizers\", \"Delegates\", \"Classes\", \"Events\", \"Enums\", \"Properties\", \"Indexers\", \"Methods\", \"Structs\", \"Interfaces\" ],\r\n  \"AccessOrder\": [ \"public\", \"internal\", \"protected internal\", \"protected\", \"private\" ],\r\n  \"StaticFirst\": true,\r\n  \"ReadonlyFirst\": true,\r\n  \"Alphabetical\": true\r\n}")
				}
			},
			FixedState =
			{
				Sources = { expectedFixedCode }
			},
			ExpectedDiagnostics = { new DiagnosticResult(CSKS002Analyzer.DiagnosticId, DiagnosticSeverity.Warning)
				.WithSpan(2, 11, 2, 24)
				.WithArguments("TestNamespace") }
		}.RunAsync();
	}

	[TestMethod]
	public async Task DefaultOrderDefaultJson()
	{
		var expectedFixedCode = @"
namespace TestNamespace
{

    // Delegate Comment
    public delegate void TestDelegate(int value);
    public delegate void TestDelegate2(int value);

    // Delegate Comment 3
    public delegate void TestDelegate3(int value);
    internal delegate void TestDelegateA(int value);

    /*
       This is a multiline
       comment
    */
    public enum TestEnum1
    {
        FirstValue,
        SecondValue
    }

    // Enum Comment
    public enum TestEnum2
    {
        FirstValue,
        SecondValue
    }

    // Interface Comment
    public interface ITestInterface
    {
    } // After Interface Comment

    // Interface Comment2
    public interface ITestInterface2
    {
    }

    // Struct Comment
    public struct TestStruct
    {
    }

    // Class Comment A
    // Class Comment B
    public class TestClassA
    {
    }
    // Class Comment
    public class TestClassC
    {
    }

    internal class TestClassB
    {
    }
}";

		await new CSharpCodeFixTest<CSKS002Analyzer,
			CSKS002CodeFixProvider, DefaultVerifier>
		{
			TestState =
			{
				Sources = { DEFAULT_INPUT_CODE }
				,AnalyzerConfigFiles = { ("/.editorconfig", @"
root = true

[*]
# Standard properties
end_of_line = lf
trim_trailing_whitespace = true
insert_final_newline = false
indent_size = 4
tab_width = 4
") }

				,
				AdditionalFiles =
				{
					("csharpkindsorter.json", "{\r\n  \"KindOrder\": [ \"ConstantFields\", \"Fields\", \"Constructors\", \"Finalizers\", \"Delegates\", \"Events\", \"Enums\", \"Interfaces\", \"Properties\", \"Indexers\", \"Methods\", \"Structs\", \"Classes\" ],\r\n  \"AccessOrder\": [ \"public\", \"internal\", \"protected internal\", \"protected\", \"private\" ],\r\n  \"StaticFirst\": true,\r\n  \"ReadonlyFirst\": true,\r\n  \"Alphabetical\": true\r\n}")
				}
			},
			FixedState =
			{
				Sources = { expectedFixedCode }
			},
			ExpectedDiagnostics = { new DiagnosticResult(CSKS002Analyzer.DiagnosticId, DiagnosticSeverity.Warning)
				.WithSpan(2, 11, 2, 24)
				.WithArguments("TestNamespace") }
		}.RunAsync();
	}

	[TestMethod]
	public async Task DefaultOrderNoJson()
	{
		var expectedFixedCode = @"
namespace TestNamespace
{

    // Delegate Comment
    public delegate void TestDelegate(int value);
    public delegate void TestDelegate2(int value);

    // Delegate Comment 3
    public delegate void TestDelegate3(int value);
    internal delegate void TestDelegateA(int value);

    /*
       This is a multiline
       comment
    */
    public enum TestEnum1
    {
        FirstValue,
        SecondValue
    }

    // Enum Comment
    public enum TestEnum2
    {
        FirstValue,
        SecondValue
    }

    // Interface Comment
    public interface ITestInterface
    {
    } // After Interface Comment

    // Interface Comment2
    public interface ITestInterface2
    {
    }

    // Struct Comment
    public struct TestStruct
    {
    }

    // Class Comment A
    // Class Comment B
    public class TestClassA
    {
    }
    // Class Comment
    public class TestClassC
    {
    }

    internal class TestClassB
    {
    }
}";

		await new CSharpCodeFixTest<CSKS002Analyzer,
			CSKS002CodeFixProvider, DefaultVerifier>
		{
			TestState =
			{
				Sources = { DEFAULT_INPUT_CODE }

				,AnalyzerConfigFiles = { ("/.editorconfig", @"
root = true

[*]
# Standard properties
end_of_line = lf
trim_trailing_whitespace = true
insert_final_newline = false
indent_size = 4
tab_width = 4
") }
			},
			FixedState =
			{
				Sources = { expectedFixedCode }
			},
			ExpectedDiagnostics = { new DiagnosticResult(CSKS002Analyzer.DiagnosticId, DiagnosticSeverity.Warning)
				.WithSpan(2, 11, 2, 24)
				.WithArguments("TestNamespace") }
		}.RunAsync();
	}

	[TestMethod]
	public async Task InternalFirst()
	{
		var expectedFixedCode = @"
namespace TestNamespace
{
    internal delegate void TestDelegateA(int value);

    // Delegate Comment
    public delegate void TestDelegate(int value);
    public delegate void TestDelegate2(int value);

    // Delegate Comment 3
    public delegate void TestDelegate3(int value);

    /*
       This is a multiline
       comment
    */
    public enum TestEnum1
    {
        FirstValue,
        SecondValue
    }

    // Enum Comment
    public enum TestEnum2
    {
        FirstValue,
        SecondValue
    }

    // Interface Comment
    public interface ITestInterface
    {
    } // After Interface Comment

    // Interface Comment2
    public interface ITestInterface2
    {
    }

    // Struct Comment
    public struct TestStruct
    {
    }

    internal class TestClassB
    {
    }

    // Class Comment A
    // Class Comment B
    public class TestClassA
    {
    }
    // Class Comment
    public class TestClassC
    {
    }
}";

		await new CSharpCodeFixTest<CSKS002Analyzer,
			CSKS002CodeFixProvider, DefaultVerifier>
		{
			TestState =
			{
				Sources = { DEFAULT_INPUT_CODE }
				,AnalyzerConfigFiles = { ("/.editorconfig", @"
root = true

[*]
# Standard properties
end_of_line = lf
trim_trailing_whitespace = true
insert_final_newline = false
indent_size = 4
tab_width = 4
") }

				,
				AdditionalFiles =
				{
					("csharpkindsorter.json", "{\r\n  \"KindOrder\": [ \"ConstantFields\", \"Fields\", \"Constructors\", \"Finalizers\", \"Delegates\", \"Events\", \"Enums\", \"Interfaces\", \"Properties\", \"Indexers\", \"Methods\", \"Structs\", \"Classes\" ],\r\n  \"AccessOrder\": [ \"internal\", \"public\", \"protected internal\", \"protected\", \"private\" ],\r\n  \"StaticFirst\": true,\r\n  \"ReadonlyFirst\": true,\r\n  \"Alphabetical\": true\r\n}")
				}
			},
			FixedState =
			{
				Sources = { expectedFixedCode }
			},
			ExpectedDiagnostics = { new DiagnosticResult(CSKS002Analyzer.DiagnosticId, DiagnosticSeverity.Warning)
				.WithSpan(2, 11, 2, 24)
				.WithArguments("TestNamespace") }
		}.RunAsync();
	}

	[TestMethod]
	public async Task InternalFirstNonAlphaSort()
	{
		var expectedFixedCode = @"
namespace TestNamespace
{
    internal delegate void TestDelegateA(int value);

    // Delegate Comment
    public delegate void TestDelegate(int value);
    public delegate void TestDelegate2(int value);

    // Delegate Comment 3
    public delegate void TestDelegate3(int value);

    // Enum Comment
    public enum TestEnum2
    {
        FirstValue,
        SecondValue
    }

    /*
       This is a multiline
       comment
    */
    public enum TestEnum1
    {
        FirstValue,
        SecondValue
    }

    // Interface Comment
    public interface ITestInterface
    {
    } // After Interface Comment

    // Interface Comment2
    public interface ITestInterface2
    {
    }

    // Struct Comment
    public struct TestStruct
    {
    }

    internal class TestClassB
    {
    }
    // Class Comment
    public class TestClassC
    {
    }

    // Class Comment A
    // Class Comment B
    public class TestClassA
    {
    }
}";

		await new CSharpCodeFixTest<CSKS002Analyzer,
			CSKS002CodeFixProvider, DefaultVerifier>
		{
			TestState =
			{
				Sources = { DEFAULT_INPUT_CODE }
				,AnalyzerConfigFiles = { ("/.editorconfig", @"
root = true

[*]
# Standard properties
end_of_line = lf
trim_trailing_whitespace = true
insert_final_newline = false
indent_size = 4
tab_width = 4
") }
				,
				AdditionalFiles =
				{
					("csharpkindsorter.json", "{\r\n  \"KindOrder\": [ \"ConstantFields\", \"Fields\", \"Constructors\", \"Finalizers\", \"Delegates\", \"Events\", \"Enums\", \"Interfaces\", \"Properties\", \"Indexers\", \"Methods\", \"Structs\", \"Classes\" ],\r\n  \"AccessOrder\": [ \"internal\", \"public\", \"protected internal\", \"protected\", \"private\" ],\r\n  \"StaticFirst\": true,\r\n  \"ReadonlyFirst\": true,\r\n  \"Alphabetical\": false\r\n}")
				}
			},
			FixedState =
			{
				Sources = { expectedFixedCode }
			},
			ExpectedDiagnostics = { new DiagnosticResult(CSKS002Analyzer.DiagnosticId, DiagnosticSeverity.Warning)
				.WithSpan(2, 11, 2, 24)
				.WithArguments("TestNamespace") }
		}.RunAsync();
	}

	[TestMethod]
	public async Task NonAlphaSort()
	{
		var expectedFixedCode = @"
namespace TestNamespace
{

    // Delegate Comment
    public delegate void TestDelegate(int value);
    public delegate void TestDelegate2(int value);

    // Delegate Comment 3
    public delegate void TestDelegate3(int value);
    internal delegate void TestDelegateA(int value);

    // Enum Comment
    public enum TestEnum2
    {
        FirstValue,
        SecondValue
    }

    /*
       This is a multiline
       comment
    */
    public enum TestEnum1
    {
        FirstValue,
        SecondValue
    }

    // Interface Comment
    public interface ITestInterface
    {
    } // After Interface Comment

    // Interface Comment2
    public interface ITestInterface2
    {
    }

    // Struct Comment
    public struct TestStruct
    {
    }
    // Class Comment
    public class TestClassC
    {
    }

    // Class Comment A
    // Class Comment B
    public class TestClassA
    {
    }

    internal class TestClassB
    {
    }
}";
		await new CSharpCodeFixTest<CSKS002Analyzer,
			CSKS002CodeFixProvider, DefaultVerifier>
		{
			TestState =
			{
				Sources = { DEFAULT_INPUT_CODE }
				,AnalyzerConfigFiles = { ("/.editorconfig", @"
root = true

[*]
# Standard properties
end_of_line = lf
trim_trailing_whitespace = true
insert_final_newline = false
indent_size = 4
tab_width = 4
") }
				,
				AdditionalFiles =
				{
					("csharpkindsorter.json", "{\r\n  \"KindOrder\": [ \"ConstantFields\", \"Fields\", \"Constructors\", \"Finalizers\", \"Delegates\", \"Events\", \"Enums\", \"Interfaces\", \"Properties\", \"Indexers\", \"Methods\", \"Structs\", \"Classes\" ],\r\n  \"AccessOrder\": [ \"public\", \"internal\", \"protected internal\", \"protected\", \"private\" ],\r\n  \"StaticFirst\": true,\r\n  \"ReadonlyFirst\": true,\r\n  \"Alphabetical\": false\r\n}")
				}
			},
			FixedState =
			{
				Sources = { expectedFixedCode }
			},
			ExpectedDiagnostics = { new DiagnosticResult(CSKS002Analyzer.DiagnosticId, DiagnosticSeverity.Warning)
				.WithSpan(2, 11, 2, 24)
				.WithArguments("TestNamespace") }
		}.RunAsync();
	}
}