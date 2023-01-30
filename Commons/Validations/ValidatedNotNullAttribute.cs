namespace ivp.edm.validations;

[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class ValidatedNotNullAttribute : Attribute
{
    // This type is used by https://rules.sonarsource.com/csharp/RSPEC-3900 to validate null checks in public methods.
}