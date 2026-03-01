namespace Content.Shared._VDS.Trigger.Systems;

public sealed partial class TriggerSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        InitializeCondition();
    }
}
