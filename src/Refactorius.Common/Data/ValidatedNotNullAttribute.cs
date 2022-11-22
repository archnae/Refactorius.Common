namespace Refactorius;

/// <summary>Signals to static analysis that, trust me, I really am checking the parameter.</summary>
/// <remarks>See http://stackoverflow.com/questions/8244958/suppress-ca1062-with-fluent-validation </remarks>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class ValidatedNotNullAttribute : Attribute
{
}