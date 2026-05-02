namespace Example.Service;

public sealed class FeatureService
{
    private volatile bool enable;

    public void UpdateFeature(bool value) => enable = value;

    public bool QueryFeature() => enable;
}
