# Exhaustive Matching for C\#

This package provides exhaustive matching for C# switch statements. That is, it can provider compiler errors for switch statements that fail to handle all the possible cases of an enum or of a type hierarchy. Checking is provided by a Roslyn code analyzer. Since an exhaustive match isn't always intended, it must be opted into. This is done by throwing specific exceptions from the default case. These exceptions provide a runtime error for any code that circumvents the compile-time exhaustiveness checking.

## Exhaustive Matching on Enum Values

Add the `ExhaustiveMatching` NuGet package to a project. To enable exhaustiveness checking for a switch on an enum, throw an `InvalidEnumArgumentException` from the default case. This exception is used to indicate that the value in an enum variable doesn't match any of the defined enum values. Thus, if the code throws it from the default case, the developer is expecting that all defined enum cases will be handled by the switch statement.

```csharp
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
}
```

Easier yet, use the convenient `ExhaustiveMatch` class in the `ExhaustiveMatching` package. The throw statement above can be replaced by `throw ExhaustiveMatch.Failed(dayOfWeek)`. This will throw an `ExhaustiveMatchFailedException` that provides both the enum type and value that wasn't matched.

## Exhaustive Matching on Type

C# 7.0 added pattern matching including the ability to switch on the type of a value. When switching on the type of a value, any value must have a subclass type that is a subclass. To be sure any value will be matched by some case clause, all these types must be matched by some case. This provides powerful case matching possibilities. The Scala language uses a similar idea in the form of "case classes" for pattern matching.

To enable exhaustiveness checking on a type match, two things must be done. The default case must throw an `ExhaustiveMatchFailedException` (using `ExhaustiveMatch.Failed(...)`) and the type being matched up must be marked with the `Matchable` attribute. Classes marked with the matchable attributes have a number of limitations imposed on them which combine to ensure the analyzer can determine the complete set of possible subclasses.

This example shows how to declare a class `Answer` that is matchable.

```csharp
[Matchable]
public abstract class Answer
{
    private protected Answer () {}
}

public sealed class Yes : Answer {}
public sealed class No : Answer {}
```

Values of type `Answer` can then be exhaustively matched against.

```csharp
switch(answer)
{
    case Yes yes:
        Console.WriteLine("The answer is Yes!");
        break;
    case No no:
        Console.WriteLine("The answer is no.");
        break;
    default:
        throw ExhaustiveMatch.Failed(answer);
}
```

## Rules for Matchable Types

1. Each class must be either:
   * `sealed`
   * Have all constructors `private`, `internal`, or `private protected` (added in C# 7.2)

**TODO:** Write out the rest of the rules

## Null Values and Exhaustiveness

**TODO**

## Rules for Matching on Type

**TODO**

## Analyzer Errors and Warnings

**TODO**

Number | Meaning
---|---
EM001 | A switch statement marked for exhaustiveness checking is not an exhaustive match.