using Content.Shared._VDS.Trigger.Components.Triggers;
using Content.Shared.Damage.Systems;
using Content.Shared.Trigger;
using Content.Shared.Trigger.Components.Triggers;

namespace Content.Shared._VDS.Trigger.Systems;

/// <summary>
/// Trigger system for being hurt.
/// </summary>
public sealed class TriggerOnHurtTriggerSystem : TriggerOnXSystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TriggerOnHurtComponent, DamageChangedEvent>(OnDamageChanged);
    }

    private void OnDamageChanged(Entity<TriggerOnHurtComponent> ent, ref DamageChangedEvent args)
    {
        if (args.DamageDelta == null)
            return;

        var target = ent.Comp.AssailantIsUser ? args.Origin : ent;
        Trigger.Trigger(ent.Owner, target, ent.Comp.KeyOut);
    }
}
