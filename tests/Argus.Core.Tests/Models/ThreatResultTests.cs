using Argus.Core.Models;
using FluentAssertions;

namespace Argus.Core.Tests.Models;

public class ThreatResultTests
{
    [Fact]
    public void ThreatResult_Clean_HasNoThreats()
    {
        var result = ThreatResult.Clean("C:\\test.exe");
        result.IsClean.Should().BeTrue();
        result.Confidence.Should().Be(0);
        result.FilePath.Should().Be("C:\\test.exe");
    }

    [Fact]
    public void ThreatResult_Malicious_RequiresEvidenceAndConfidence()
    {
        var result = ThreatResult.Malicious("C:\\bad.exe", "YARA:Ransomware.Generic", 95);
        result.IsClean.Should().BeFalse();
        result.IsMalicious.Should().BeTrue();
        result.Confidence.Should().Be(95);
        result.Evidence.Should().Be("YARA:Ransomware.Generic");
    }

    [Fact]
    public void ThreatResult_Suspicious_IsNeitherCleanNorMalicious()
    {
        var result = ThreatResult.Suspicious("C:\\maybe.exe", "HighEntropy", 55);
        result.IsClean.Should().BeFalse();
        result.IsMalicious.Should().BeFalse();
        result.IsSuspicious.Should().BeTrue();
    }

    [Fact]
    public void ThreatResult_Unknown_FailClosed_IsNotClean()
    {
        // SECURITY: Scanner errors must NEVER return Clean.
        var result = ThreatResult.Unknown("C:\\error.exe", "AMSI threw AccessViolation");
        result.IsClean.Should().BeFalse();
        result.IsUnknown.Should().BeTrue();
        result.Evidence.Should().Contain("ScanError");
    }
}
