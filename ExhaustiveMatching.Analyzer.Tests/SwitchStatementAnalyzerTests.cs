using ExhaustiveMatching.Analyzer.Tests.Helpers;
using ExhaustiveMatching.Analyzer.Tests.Verifiers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExhaustiveMatching.Analyzer.Tests
{
    [TestClass]
    public class SwitchStatementAnalyzerTests : CodeFixVerifier
    {
        [TestMethod]
        public void SwitchOnEnumThrowingInvalidEnumIsNotExhaustiveReportsDiagnostic()
        {
            const string args = "DayOfWeek dayOfWeek";
            const string test = @"
        switch(dayOfWeek)
        {
            case DayOfWeek.Monday:
            case DayOfWeek.Tuesday:
            case DayOfWeek.Wednesday:
            case DayOfWeek.Thursday:
            case DayOfWeek.Friday:
                Console.WriteLine(""Weekday"");
            break;
            case DayOfWeek.Saturday:
                // Omitted Sunday
                Console.WriteLine(""Weekend"");
                break;
            default:
                throw new InvalidEnumArgumentException(nameof(dayOfWeek), (int)dayOfWeek, typeof(DayOfWeek));
        }";

            var expected = new DiagnosticResult
            {
                Id = "EM001",
                Message = "Some values of the enum are not processed by switch: Sunday",
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 10, 9)
                    }
            };

            VerifyCSharpDiagnostic(CodeContext(args, test), expected);
        }

        [TestMethod]
        public void SwitchOnEnumThrowingExhaustiveMatchFailedIsNotExhaustiveReportsDiagnostic()
        {
            const string args = "DayOfWeek dayOfWeek";
            const string test = @"
        switch(dayOfWeek)
        {
            case DayOfWeek.Monday:
            case DayOfWeek.Tuesday:
            case DayOfWeek.Wednesday:
            case DayOfWeek.Thursday:
            case DayOfWeek.Friday:
                Console.WriteLine(""Weekday"");
            break;
            case DayOfWeek.Saturday:
                // Omitted Sunday
                Console.WriteLine(""Weekend"");
                break;
            default:
                throw ExhaustiveMatch.Failed(dayOfWeek);
        }";

            var expected = new DiagnosticResult
            {
                Id = "EM001",
                Message = "Some values of the enum are not processed by switch: Sunday",
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 10, 9)
                    }
            };

            VerifyCSharpDiagnostic(CodeContext(args, test), expected);
        }

        [TestMethod]
        public void SwitchOnClosedThrowingExhaustiveMatchFailedIsNotExhaustiveReportsDiagnostic()
        {
            const string args = "Shape shape";
            const string test = @"
        switch (shape)
        {
            case Square square:
                Console.WriteLine(""Square: "" + square);
                break;
            case Circle circle:
                Console.WriteLine(""Circle: "" + circle);
                break;
            default:
                throw ExhaustiveMatch.Failed(shape);
        }";

            var expected = new DiagnosticResult
            {
                Id = "EM002",
                Message = "Some subtypes are not processed by switch: TestNamespace.Triangle",
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 10, 9)
                    }
            };

            VerifyCSharpDiagnostic(CodeContext(args, test), expected);
        }

        [TestMethod]
        public void ExhaustiveObjectSwitchAllowsNull()
        {
            const string args = "Shape shape";
            const string test = @"
        switch (shape)
        {
            case Square square:
                Console.WriteLine(""Square: "" + square);
                break;
            case Circle circle:
                Console.WriteLine(""Circle: "" + circle);
                break;
            case Triangle triangle:
                Console.WriteLine(""Triangle: "" + triangle);
                break;
            default:
                throw ExhaustiveMatch.Failed(shape);
        }";

            VerifyCSharpDiagnostic(CodeContext(args, test));
        }

        [TestMethod]
        public void UnsupportedCaseClauses()
        {
            const string args = "Shape shape";
            const string test = @"
        switch (shape)
        {
            case Square square:
                Console.WriteLine(""Square: "" + square);
                break;
            case Circle circle:
                Console.WriteLine(""Circle: "" + circle);
                break;
            case Triangle triangle when true:
                Console.WriteLine(""Triangle: "" + triangle);
                break;
            case 12:
                break;
            default:
                throw ExhaustiveMatch.Failed(shape);
        }";

            var expected1 = new DiagnosticResult
            {
                Id = "EM100",
                Message = "When clauses are not supported in an exhaustive switch",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 18, 36, 9) }
            };

            var expected2 = new DiagnosticResult
            {
                Id = "EM101",
                Message = "Case clause type not supported in exhaustive switch: case 12:",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 21, 13) }
            };

            VerifyCSharpDiagnostic(CodeContext(args, test), expected1, expected2);
        }

        private static string CodeContext(string args, string body)
        {
            const string context = @"using System; // DayOfWeek
using System.ComponentModel; // InvalidEnumArgumentException
using ExhaustiveMatching;
using TestNamespace;

class TestClass
{{
    void TestMethod({0})
    {{{1}
    }}
}}

namespace TestNamespace
{{
    [Closed(
        typeof(Square),
        typeof(Circle),
        typeof(Triangle))]
    public abstract class Shape {{ }}
    public sealed class Square : Shape {{ }}
    public sealed class Circle : Shape {{ }}
    public sealed class Triangle : Shape {{ }}
}}";
            return string.Format(context, args, body);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new Analyzer.ExhaustiveMatchAnalyzer();
        }
    }
}