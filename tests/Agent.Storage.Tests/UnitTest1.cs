using Agent.Storage.Identity;

namespace Agent.Storage.Tests;

public class AgentIdentityTests
{
    [Fact]
    public void GetDeviceId_ShouldReturnSameIdAcrossCalls()
    {
        var identityFilePath = Path.Combine(
            Path.GetTempPath(),
            $"agent-identity-{Guid.NewGuid()}.txt");

        var identity = new AgentIdentity(identityFilePath);

        var firstId = identity.GetDeviceId();
        var secondId = identity.GetDeviceId();

        Assert.False(string.IsNullOrWhiteSpace(firstId));
        Assert.Equal(firstId, secondId);

        File.Delete(identityFilePath);
    }
}