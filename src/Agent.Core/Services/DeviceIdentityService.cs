namespace Agent.Core.Services;

public class DeviceIdentityService
{
    private readonly string _identityFilePath;

    public DeviceIdentityService(string identityFilePath)
    {
        _identityFilePath = identityFilePath;
    }

    public string GetDeviceId()
    {
        if (File.Exists(_identityFilePath))
        {
            var existingId = File.ReadAllText(_identityFilePath).Trim();

            if (!string.IsNullOrWhiteSpace(existingId))
            {
                return existingId;
            }
        }

        var deviceId = $"AGT-{Guid.NewGuid()}";

        var directory = Path.GetDirectoryName(_identityFilePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(_identityFilePath, deviceId);

        return deviceId;
    }
}