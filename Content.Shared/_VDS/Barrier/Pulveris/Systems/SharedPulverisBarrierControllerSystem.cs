namespace Content.Shared._VDS.Barrier.Pulveris.Systems;

public abstract class SharedPulverisBarrierControllerSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    // private void UpdateController(Entity<PulverisBarrierControllerComponent> controller)
    // {
    //
    // }
    public bool TryUpdateAppearance(Entity<PulverisBarrierControllerComponent?> ent, AppearanceComponent? appearance = null)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return false;

        if (!Resolve(ent, ref appearance))
            return false;

        _appearance.SetData(ent.Owner, PulverisBarrierControllerVisuals.Connected, ent.Comp.Connected, appearance);

        return true;
    }
}
