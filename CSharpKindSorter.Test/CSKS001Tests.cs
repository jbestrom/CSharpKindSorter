using System.Threading.Tasks;
using CSharpKindSorter.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpKindSorter.Test;

[TestClass]
public class CSKS001Tests : CSharpCodeFixVerifier<CSKS001Analyzer, CSKS001CodeFixProvider, DefaultVerifier>
{
	private const string DEFAULT_CLASS_CODE = @"
using System;

class MyClass
{
    public void DoSomethingC()                       // Regular method
    {
        // Do something
    }

    // Fields
    private readonly int _readonlyField;            // Readonly field
    private int _mutableField;                      // Mutable field

    // Constants
    public const string ConstField = ""constant"";    // Constant field

    // Static Fields
    public static int StaticField;                  // Static field

    // Properties
    public string ReadOnlyProperty { get; }         // Read-only property
    public int Property { get; set; }               // Regular property
    protected internal double ProtectedProperty { get; set; }  // Protected internal property

    // Event Field
    public event EventHandler MyEventField;              // Event Field

    // Event Block
    public event EventHandler MyEventBlock { add => MyEventField += value; remove => MyEventField -= value; }              // Event Block

    // Delegates
    public delegate void MyDelegate(string message); // Delegate

    // Constructors
    public MyClass()                                // Constructor
    {
        _readonlyField = 42;
    }

    // Static Constructor
    static MyClass()                                // Static constructor
    {
        StaticField = 100;
    }

    // Destructor (Finalizer)
    ~MyClass()                                      // Destructor
    {
        // Cleanup code
    }

    // Methods
    public void DoSomethingB()                       // Regular method
    {
        // Do something
    }

    public void DoSomethingA()                       // Regular method
    {
        // Do something
    }

    private static void StaticMethod()              // Static method
    {
        // Static method logic
    }

    private int PrivateMethod()                     // Private method
    {
        return _mutableField;
    }

    // Nested Class
    private class NestedClass                       // Nested class
    {
        public void NestedMethod()                  // Method in nested class
        {
            // Do something
        }
    }

    protected internal void ProtectedInternalMethod() // Protected internal method
    {
        // Protected internal logic
    }
}";

	[TestMethod]
	public async Task DefaultOrderNoJson()
	{
		var fixedCode = @"
using System;

class MyClass
{

    // Constants
    public const string ConstField = ""constant"";    // Constant field

    // Static Fields
    public static int StaticField;                  // Static field

    // Fields
    private readonly int _readonlyField;            // Readonly field
    private int _mutableField;                      // Mutable field

    // Static Constructor
    static MyClass()                                // Static constructor
    {
        StaticField = 100;
    }

    // Constructors
    public MyClass()                                // Constructor
    {
        _readonlyField = 42;
    }

    // Destructor (Finalizer)
    ~MyClass()                                      // Destructor
    {
        // Cleanup code
    }

    // Delegates
    public delegate void MyDelegate(string message); // Delegate

    // Event Field
    public event EventHandler MyEventField;              // Event Field

    // Event Block
    public event EventHandler MyEventBlock { add => MyEventField += value; remove => MyEventField -= value; }              // Event Block
    public int Property { get; set; }               // Regular property

    // Properties
    public string ReadOnlyProperty { get; }         // Read-only property
    protected internal double ProtectedProperty { get; set; }  // Protected internal property

    public void DoSomethingA()                       // Regular method
    {
        // Do something
    }

    // Methods
    public void DoSomethingB()                       // Regular method
    {
        // Do something
    }
    public void DoSomethingC()                       // Regular method
    {
        // Do something
    }

    protected internal void ProtectedInternalMethod() // Protected internal method
    {
        // Protected internal logic
    }

    private static void StaticMethod()              // Static method
    {
        // Static method logic
    }

    private int PrivateMethod()                     // Private method
    {
        return _mutableField;
    }

    // Nested Class
    private class NestedClass                       // Nested class
    {
        public void NestedMethod()                  // Method in nested class
        {
            // Do something
        }
    }
}";

		await new CSharpCodeFixTest<CSKS001Analyzer, CSKS001CodeFixProvider, DefaultVerifier>
		{
			TestState =
			{
				Sources = { DEFAULT_CLASS_CODE }
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
				Sources = { fixedCode }
			},
			ExpectedDiagnostics = {
				new DiagnosticResult(CSKS001Analyzer.DiagnosticId, DiagnosticSeverity.Warning).WithSpan(4, 1, 87, 2).WithArguments("MyClass"),
			}
		}.RunAsync();
	}

	[TestMethod]
	public async Task DefaultOrderNoJsonMultiInNamespace()
	{
		var inputCode = @"
namespace TestNamespace
{
    class FirstClass
    {
        private int ZField;
        public void DoSomething() { }
        private int AField;
        public string Name { get; }
    }

    struct MyStruct
    {
        private int CField;
        private int BField;
        public void DoStructStuff() { }
    }

    interface IMyInterface
    {
        void MethodB();
        string PropertyA { get; }
    }
}";

		var fixedCode = @"
namespace TestNamespace
{
    class FirstClass
    {
        private int AField;
        private int ZField;
        public string Name { get; }
        public void DoSomething() { }
    }

    struct MyStruct
    {
        private int BField;
        private int CField;
        public void DoStructStuff() { }
    }

    interface IMyInterface
    {
        string PropertyA { get; }
        void MethodB();
    }
}";

		await new CSharpCodeFixTest<CSKS001Analyzer, CSKS001CodeFixProvider, DefaultVerifier>
		{
			TestState =
			{
				Sources = { inputCode }
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
				Sources = { fixedCode }
			},
			ExpectedDiagnostics = {
				new DiagnosticResult(CSKS001Analyzer.DiagnosticId, DiagnosticSeverity.Warning).WithSpan(4, 5, 10, 6).WithArguments("FirstClass"),
				new DiagnosticResult(CSKS001Analyzer.DiagnosticId, DiagnosticSeverity.Warning).WithSpan(12, 5, 17, 6).WithArguments("MyStruct"),
				new DiagnosticResult(CSKS001Analyzer.DiagnosticId, DiagnosticSeverity.Warning).WithSpan(19, 5, 23, 6).WithArguments("IMyInterface"),
			}
		}.RunAsync();
	}

	[TestMethod]
	public async Task ExplicitTest()
	{
		var inputCode = @"
using System;

namespace TestNamespace
{
interface ITest
   {
      int TestA();
      int TestB();
   }

   class MyClass : ITest
   {
      int ITest.TestB(){ return 1; }
      public int TestA(){ return 1; }
      public int TestC(){ return 1; }
      private int _ATest(){ return 1; }
   }
}";

		var fixedCode = @"
using System;

namespace TestNamespace
{
    interface ITest
    {
        int TestA();
        int TestB();
    }

    class MyClass : ITest
    {
        public int TestA() { return 1; }
        public int TestC() { return 1; }
        int ITest.TestB() { return 1; }
        private int _ATest() { return 1; }
    }
}";

		await new CSharpCodeFixTest<CSKS001Analyzer, CSKS001CodeFixProvider, DefaultVerifier>
		{
			TestState =
			{
				Sources = { inputCode },
				AnalyzerConfigFiles = { ("/.editorconfig", @"
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
				Sources = { fixedCode }
			},
			ExpectedDiagnostics = {
				new DiagnosticResult(CSKS001Analyzer.DiagnosticId, DiagnosticSeverity.Warning).WithSpan(12, 4, 18, 5).WithArguments("MyClass")
			}
		}.RunAsync();
	}

	[TestMethod]
	public async Task ExpressionBodyTest()
	{
		var inputCode = @"
using System;

namespace TestNamespace
{
   class MyClass
   {
      private string _stringA;
      private string _stringB;

      private string TestB => _stringB ?? (_stringB = ""StringB"");
      private string TestA => _stringA ?? (_stringA = ""StringA"");
   }
}";

		var fixedCode = @"
using System;

namespace TestNamespace
{
    class MyClass
    {
        private string _stringA;
        private string _stringB;
        private string TestA => _stringA ?? (_stringA = ""StringA"");

        private string TestB => _stringB ?? (_stringB = ""StringB"");
    }
}";
		await new CSharpCodeFixTest<CSKS001Analyzer, CSKS001CodeFixProvider, DefaultVerifier>
		{
			TestState =
			{
				Sources = { inputCode }

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
				Sources = { fixedCode }
			},
			ExpectedDiagnostics = {
				new DiagnosticResult(CSKS001Analyzer.DiagnosticId, DiagnosticSeverity.Warning).WithSpan(6, 4, 13, 5).WithArguments("MyClass")
			}
		}.RunAsync();
	}

	[TestMethod]
	public async Task DefaultOrderNoAlpha()
	{
		var fixedCode = @"
using System;

class MyClass
{

    // Constants
    public const string ConstField = ""constant"";    // Constant field

    // Static Fields
    public static int StaticField;                  // Static field

    // Fields
    private readonly int _readonlyField;            // Readonly field
    private int _mutableField;                      // Mutable field

    // Static Constructor
    static MyClass()                                // Static constructor
    {
        StaticField = 100;
    }

    // Constructors
    public MyClass()                                // Constructor
    {
        _readonlyField = 42;
    }

    // Destructor (Finalizer)
    ~MyClass()                                      // Destructor
    {
        // Cleanup code
    }

    // Delegates
    public delegate void MyDelegate(string message); // Delegate

    // Event Field
    public event EventHandler MyEventField;              // Event Field

    // Event Block
    public event EventHandler MyEventBlock { add => MyEventField += value; remove => MyEventField -= value; }              // Event Block

    // Properties
    public string ReadOnlyProperty { get; }         // Read-only property
    public int Property { get; set; }               // Regular property
    protected internal double ProtectedProperty { get; set; }  // Protected internal property
    public void DoSomethingC()                       // Regular method
    {
        // Do something
    }

    // Methods
    public void DoSomethingB()                       // Regular method
    {
        // Do something
    }

    public void DoSomethingA()                       // Regular method
    {
        // Do something
    }

    protected internal void ProtectedInternalMethod() // Protected internal method
    {
        // Protected internal logic
    }

    private static void StaticMethod()              // Static method
    {
        // Static method logic
    }

    private int PrivateMethod()                     // Private method
    {
        return _mutableField;
    }

    // Nested Class
    private class NestedClass                       // Nested class
    {
        public void NestedMethod()                  // Method in nested class
        {
            // Do something
        }
    }
}";

		await new CSharpCodeFixTest<CSKS001Analyzer, CSKS001CodeFixProvider, DefaultVerifier>
		{
			TestState =
			{
				Sources = { DEFAULT_CLASS_CODE }
				,AnalyzerConfigFiles = { ("/.editorconfig", @"
root = true

[*]
# Standard properties
end_of_line = lf
trim_trailing_whitespace = true
insert_final_newline = false
indent_size = 4
tab_width = 4
") },
				AdditionalFiles =
				{
				("csharpkindsorter.json", "{\r\n  \"KindOrder\": [ \"ConstantFields\", \"Fields\", \"Constructors\", \"Finalizers\", \"Delegates\", \"Events\", \"Enums\", \"Interfaces\", \"Properties\", \"Indexers\", \"Methods\", \"Structs\", \"Classes\" ],\r\n  \"AccessOrder\": [ \"internal\", \"public\", \"protected internal\", \"protected\", \"private\" ]," +
										  "\r\n  \"StaticFirst\": true,\r\n  \"ReadonlyFirst\": true,\r\n  \"Alphabetical\": false\r\n}")
			}
			},
			FixedState =
			{
				Sources = { fixedCode }
			},
			ExpectedDiagnostics = {
				new DiagnosticResult(CSKS001Analyzer.DiagnosticId, DiagnosticSeverity.Warning).WithSpan(4, 1, 87, 2).WithArguments("MyClass"),
			}
		}.RunAsync();
	}



	[TestMethod]
	public async Task PrivateConstvsStaticReadonlyTest001()
	{
		var inputCode = @"
using System;
using System.Collections.Generic;

namespace TestNamespace
{
    class MyClass
    {
        public const string A = ""A"";
        public const string B = ""B"";

        private const string CA = ""CA"";
        private const string CB = ""CB"";

        internal static readonly Dictionary<int, string> IntDictionary = new Dictionary<int, string> { { 1, ""A"" }, { 2, ""B"" } };
    }
}";

		var fixedCode = @"
using System;
using System.Collections.Generic;

namespace TestNamespace
{
    class MyClass
    {
        public const string A = ""A"";
        public const string B = ""B"";

        internal static readonly Dictionary<int, string> IntDictionary = new Dictionary<int, string> { { 1, ""A"" }, { 2, ""B"" } };

        private const string CA = ""CA"";
        private const string CB = ""CB"";
    }
}";
		await new CSharpCodeFixTest<CSKS001Analyzer, CSKS001CodeFixProvider, DefaultVerifier>
		{
			TestState =
			{
				Sources = { inputCode }
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
				Sources = { fixedCode }
			},
			ExpectedDiagnostics = {
				new DiagnosticResult(CSKS001Analyzer.DiagnosticId, DiagnosticSeverity.Warning).WithSpan(7, 5, 16, 6).WithArguments("MyClass")
			}
		}.RunAsync();
	}


	[TestMethod]
	public async Task PrivateConstvsStaticReadonlyTest002()
	{
		var inputCode = @"
using System;
using System.Collections.Generic;

namespace TestNamespace
{
    class MyClass
    {
        public const string A = ""A"";
        public const string B = ""B"";

        internal static readonly Dictionary<int, string> IntDictionary = new Dictionary<int, string> { { 1, ""A"" }, { 2, ""B"" } };

        private const string CA = ""CA"";
        private const string CB = ""CB"";
    }
}";

		var fixedCode = @"
using System;
using System.Collections.Generic;

namespace TestNamespace
{
    class MyClass
    {
        public const string A = ""A"";
        public const string B = ""B"";

        internal static readonly Dictionary<int, string> IntDictionary = new Dictionary<int, string> { { 1, ""A"" }, { 2, ""B"" } };

        private const string CA = ""CA"";
        private const string CB = ""CB"";
    }
}";
		await new CSharpCodeFixTest<CSKS001Analyzer, CSKS001CodeFixProvider, DefaultVerifier>
		{
			TestState =
			{
				Sources = { inputCode }

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
				Sources = { fixedCode }
			},

		}.RunAsync();
	}


	[TestMethod]
	public async Task FieldTest001()
	{
		var inputCode = @"
using System;
using System.Collections.Generic;

namespace TestNamespace
{
    class MyClass
    {
        private readonly string ApiKey = ""apiKey"";
        private readonly string AppId = ""appId"";
        private readonly int VersionNumber;
        private const string URI = ""test.com"";
    }
}";

		var fixedCode = @"
using System;
using System.Collections.Generic;

namespace TestNamespace
{
    class MyClass
    {
        private const string URI = ""test.com"";
        private readonly string ApiKey = ""apiKey"";
        private readonly string AppId = ""appId"";
        private readonly int VersionNumber;
    }
}";
		await new CSharpCodeFixTest<CSKS001Analyzer, CSKS001CodeFixProvider, DefaultVerifier>
		{
			TestState =
			{
				Sources = { inputCode }
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
				Sources = { fixedCode }
			},
			ExpectedDiagnostics = {
				new DiagnosticResult(CSKS001Analyzer.DiagnosticId, DiagnosticSeverity.Warning).WithSpan(7, 5, 13, 6).WithArguments("MyClass")
			}
		}.RunAsync();
	}

	[TestMethod]
	public async Task FieldTest002()
	{
		var inputCode = @"
using System;
using System.Collections.Generic;

namespace TestNamespace
{
    class MyClass
    {
    private int _mutableField;                      // Mutable field

 // Fields
    private readonly int _readonlyField;            // Readonly field

    // Static Fields
    public static int StaticField;                  // Static field

    // Event Field
    public event EventHandler MyEventField;              // Event Field

    }
}";

		var fixedCode = @"
using System;
using System.Collections.Generic;

namespace TestNamespace
{
    class MyClass
    {

        // Static Fields
        public static int StaticField;                  // Static field

        // Fields
        private readonly int _readonlyField;            // Readonly field
        private int _mutableField;                      // Mutable field

        // Event Field
        public event EventHandler MyEventField;              // Event Field

    }
}";
		await new CSharpCodeFixTest<CSKS001Analyzer, CSKS001CodeFixProvider, DefaultVerifier>
		{
			TestState =
			{
				Sources = { inputCode }
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
				Sources = { fixedCode }
			},
			ExpectedDiagnostics = {
				new DiagnosticResult(CSKS001Analyzer.DiagnosticId, DiagnosticSeverity.Warning).WithSpan(7, 5, 20, 6).WithArguments("MyClass")
			}
		}.RunAsync();
	}



	[TestMethod]
	public async Task StaticTest001()
	{
		var inputCode = @"
using System;
using System.Collections.Generic;

namespace TestNamespace
{
    interface ITestInterface {}

    class MyClass
    {
        private static readonly IDictionary<Type, Func<ITestInterface, Guid>> _cT = new Dictionary<Type, Func<ITestInterface, Guid>>();
        private static Guid? _lTId;
        private readonly ITestInterface _dInterface;
        private static string _lTLI;

        static MyClass()
        {
        }

        public MyClass(ITestInterface test)
        {
            _dInterface = test;
        }
    }
}";

		var fixedCode = @"
using System;
using System.Collections.Generic;

namespace TestNamespace
{
    interface ITestInterface { }

    class MyClass
    {
        private static readonly IDictionary<Type, Func<ITestInterface, Guid>> _cT = new Dictionary<Type, Func<ITestInterface, Guid>>();
        private static Guid? _lTId;
        private static string _lTLI;
        private readonly ITestInterface _dInterface;

        static MyClass()
        {
        }

        public MyClass(ITestInterface test)
        {
            _dInterface = test;
        }
    }
}";
		await new CSharpCodeFixTest<CSKS001Analyzer, CSKS001CodeFixProvider, DefaultVerifier>
		{
			TestState =
			{
				Sources = { inputCode }
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
				Sources = { fixedCode }
			},
			ExpectedDiagnostics = {
				new DiagnosticResult(CSKS001Analyzer.DiagnosticId, DiagnosticSeverity.Warning).WithSpan(9, 5, 24, 6).WithArguments("MyClass")
			}
		}.RunAsync();
	}



	[TestMethod]
	public async Task OperatorTest001()
	{
		var inputCode = @"
using System;
using System.Collections.Generic;

namespace TestNamespace
{
    class MyClass<TId> : IEquatable<MyClass<TId>>
        where TId : struct
    {
        public byte[] ByteArray { get; private set; }

        public TId Id { get; private set; }

        public static bool operator !=(MyClass<TId> left, MyClass<TId> right)
        {
            return !(left == right);
        }

        public static bool operator ==(MyClass<TId> left, MyClass<TId> right)
        {
            return EqualityComparer<MyClass<TId>>.Default.Equals(left, right);
        }

        public MyClass(TId id, byte[] byteArray)
        {
            Id = id;
            ByteArray = byteArray;
        }


public bool Equals(MyClass<TId> other)
{
    return other != null &&
           EqualityComparer<TId>.Default.Equals(Id, other.Id) &&
           EqualityComparer<byte[]>.Default.Equals(ByteArray, other.ByteArray);
}
    }
}";

		var fixedCode = @"
using System;
using System.Collections.Generic;

namespace TestNamespace
{
    class MyClass<TId> : IEquatable<MyClass<TId>>
        where TId : struct
    {

        public MyClass(TId id, byte[] byteArray)
        {
            Id = id;
            ByteArray = byteArray;
        }
        public byte[] ByteArray { get; private set; }

        public TId Id { get; private set; }

        public static bool operator !=(MyClass<TId> left, MyClass<TId> right)
        {
            return !(left == right);
        }

        public static bool operator ==(MyClass<TId> left, MyClass<TId> right)
        {
            return EqualityComparer<MyClass<TId>>.Default.Equals(left, right);
        }


        public bool Equals(MyClass<TId> other)
        {
            return other != null &&
                   EqualityComparer<TId>.Default.Equals(Id, other.Id) &&
                   EqualityComparer<byte[]>.Default.Equals(ByteArray, other.ByteArray);
        }
    }
}";
		await new CSharpCodeFixTest<CSKS001Analyzer, CSKS001CodeFixProvider, DefaultVerifier>
		{
			TestState =
			{
				Sources = { inputCode }
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
				Sources = { fixedCode }
			},
			ExpectedDiagnostics = {
				new DiagnosticResult(CSKS001Analyzer.DiagnosticId, DiagnosticSeverity.Warning).WithSpan(7, 5, 37, 6).WithArguments("MyClass")
			}
		}.RunAsync();
	}


	[TestMethod]
	public async Task OperatorTest002()
	{
		var inputCode = @"
using System;
using System.Collections.Generic;

namespace TestNamespace
{
    class MyClass<TId> : IEquatable<MyClass<TId>>
        where TId : struct
    {
        public byte[] ByteArray { get; private set; }

        public TId Id { get; private set; }

        public static bool operator !=(MyClass<TId> left, MyClass<TId> right) => !(left == right);
        
        public static bool operator ==(MyClass<TId> left, MyClass<TId> right) => EqualityComparer<MyClass<TId>>.Default.Equals(left, right);
        
        public MyClass(TId id, byte[] byteArray)
        {
            Id = id;
            ByteArray = byteArray;
        }


public bool Equals(MyClass<TId> other)
{
    return other != null &&
           EqualityComparer<TId>.Default.Equals(Id, other.Id) &&
           EqualityComparer<byte[]>.Default.Equals(ByteArray, other.ByteArray);
}
    }
}";

		var fixedCode = @"
using System;
using System.Collections.Generic;

namespace TestNamespace
{
    class MyClass<TId> : IEquatable<MyClass<TId>>
        where TId : struct
    {

        public MyClass(TId id, byte[] byteArray)
        {
            Id = id;
            ByteArray = byteArray;
        }
        public byte[] ByteArray { get; private set; }

        public TId Id { get; private set; }

        public static bool operator !=(MyClass<TId> left, MyClass<TId> right) => !(left == right);

        public static bool operator ==(MyClass<TId> left, MyClass<TId> right) => EqualityComparer<MyClass<TId>>.Default.Equals(left, right);


        public bool Equals(MyClass<TId> other)
        {
            return other != null &&
                   EqualityComparer<TId>.Default.Equals(Id, other.Id) &&
                   EqualityComparer<byte[]>.Default.Equals(ByteArray, other.ByteArray);
        }
    }
}";
		await new CSharpCodeFixTest<CSKS001Analyzer, CSKS001CodeFixProvider, DefaultVerifier>
		{
			TestState =
			{
				Sources = { inputCode }
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
				Sources = { fixedCode }
			},
			ExpectedDiagnostics = {
				new DiagnosticResult(CSKS001Analyzer.DiagnosticId, DiagnosticSeverity.Warning).WithSpan(7, 5, 31, 6).WithArguments("MyClass")
			}
		}.RunAsync();
	}


	[TestMethod]
	public async Task InternalTest001()
	{
		var inputCode = @"
using System;
using System.Collections.Generic;

namespace TestNamespace
{
public class AuthClass {
        internal virtual ApiEngine ApiEngine => new ApiEngine();
        internal virtual int Authentication => 1;
}

    public class ApiEngine { }

    internal class MyClass : AuthClass
    {
        internal override int Authentication => 2;
        internal override ApiEngine ApiEngine => new ApiEngine();
        protected internal ApiEngine ApiEngineDependency { get; } = new ApiEngine();
        protected internal AuthClass AuthenticationDependency { get; } = new AuthClass();
    }
}";

		var fixedCode = @"
using System;
using System.Collections.Generic;

namespace TestNamespace
{
    public class AuthClass
    {
        internal virtual ApiEngine ApiEngine => new ApiEngine();
        internal virtual int Authentication => 1;
    }

    public class ApiEngine { }

    internal class MyClass : AuthClass
    {
        internal override ApiEngine ApiEngine => new ApiEngine();
        internal override int Authentication => 2;
        protected internal ApiEngine ApiEngineDependency { get; } = new ApiEngine();
        protected internal AuthClass AuthenticationDependency { get; } = new AuthClass();
    }
}";
		await new CSharpCodeFixTest<CSKS001Analyzer, CSKS001CodeFixProvider, DefaultVerifier>
		{
			TestState =
			{
				Sources = { inputCode }
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
				Sources = { fixedCode }
			},
			ExpectedDiagnostics = {
				new DiagnosticResult(CSKS001Analyzer.DiagnosticId, DiagnosticSeverity.Warning).WithSpan(14, 5, 20, 6).WithArguments("MyClass")
			}
		}.RunAsync();
	}
}