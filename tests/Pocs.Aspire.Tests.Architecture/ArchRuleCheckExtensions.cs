namespace Pocs.Aspire.Tests.Architecture;

using System.Linq;
using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Fluent.Extensions;
using Shouldly;

// The core TngTech.ArchUnitNET package (0.13.4) exposes IArchRule.Evaluate/HasNoViolations
// but no throwing Check(architecture); that convenience lives only in the per-test-framework
// extension packages (TngTech.ArchUnitNET.xUnitV3 etc.). This wraps the core primitives so the
// tests can call rule.Check(architecture) without taking on that extra package dependency.
internal static class ArchRuleCheckExtensions
{
    internal static void Check(this IArchRule rule, Architecture architecture)
    {
        var violations = rule.Evaluate(architecture).Where(result => !result.Passed).ToList();

        violations.ShouldBeEmpty(EvaluationResultExtensions.ToErrorMessage(violations));
    }
}
