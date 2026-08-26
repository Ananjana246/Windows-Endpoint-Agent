namespace Agent.Storage.Identity;
public class AgentIdentity
{
    private readonly string _identityFilePath;
    public AgentIdentity(string identityFilePath)
    {
        _identityFilePath = identityFilePath;
    }
    public string GetDeviceId()
    {
        if (File.Exists(_identityFilePath))
        {
            return File.ReadAllText(_identityFilePath).Trim();
        }
        var deviceId = Guid.NewGuid().ToString();
        var directory = Path.GetDirectoryName(_identityFilePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
        File.WriteAllText(_identityFilePath, deviceId);
        return deviceId;
    }
}