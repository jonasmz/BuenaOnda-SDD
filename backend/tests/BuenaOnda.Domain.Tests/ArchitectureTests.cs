using System.Reflection;
using BuenaOnda.Application.Catalog.Ports;
using BuenaOnda.Domain.Catalog;

namespace BuenaOnda.Domain.Tests;

/// <summary>Principio III: el núcleo no conoce la persistencia ni los adaptadores.</summary>
public class ArchitectureTests
{
    private static readonly string[] Forbidden = ["Microsoft.EntityFrameworkCore", "Npgsql", "Microsoft.AspNetCore"];

    private static IEnumerable<string> References(Assembly assembly) =>
        assembly.GetReferencedAssemblies().Select(a => a.Name!);

    [Fact]
    public void Domain_references_no_project_or_package_beyond_the_framework()
    {
        var references = References(typeof(Category).Assembly).ToList();

        Assert.DoesNotContain(references, r => r.StartsWith("BuenaOnda", StringComparison.Ordinal));
        Assert.DoesNotContain(references, r => Forbidden.Any(f => r.StartsWith(f, StringComparison.Ordinal)));
    }

    [Fact]
    public void Application_references_only_the_domain_and_no_persistence()
    {
        var references = References(typeof(ICategoryRepository).Assembly).ToList();

        Assert.DoesNotContain(references, r => r.StartsWith("BuenaOnda", StringComparison.Ordinal) && r != "BuenaOnda.Domain");
        Assert.DoesNotContain(references, r => Forbidden.Any(f => r.StartsWith(f, StringComparison.Ordinal)));
    }
}
