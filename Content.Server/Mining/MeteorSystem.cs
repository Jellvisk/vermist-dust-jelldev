using Content.Server.Administration.Logs;
using Content.Server.Destructible;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Database;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs.Systems;
using Content.Shared.Projectiles;
using Robust.Shared.Physics.Events;
using Robust.Shared.Player;

namespace Content.Server.Mining;

public sealed class MeteorSystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly DestructibleSystem _destructible = default!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<MeteorComponent, ProjectileHitEvent>(OnCollide);
    }

    private void OnCollide(EntityUid uid, MeteorComponent component, ref ProjectileHitEvent args)
    {
        if (TerminatingOrDeleted(args.Target) || TerminatingOrDeleted(uid))
            return;

        if (component.HitList.Contains(args.Target))
            return;

        FixedPoint2 threshold;
        if (_mobThreshold.TryGetDeadThreshold(args.Target, out var mobThreshold))
        {
            threshold = mobThreshold.Value;
            if (HasComp<ActorComponent>(args.Target))
                _adminLog.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(args.Target):player} was struck by meteor {ToPrettyString(uid):ent} and killed instantly.");
        }
        else if (_destructible.TryGetDestroyedAt(args.Target, out var destroyThreshold))
        {
            threshold = destroyThreshold.Value;
        }
        else
        {
            threshold = FixedPoint2.MaxValue;
        }
        var otherEntDamage = CompOrNull<DamageableComponent>(args.Target)?.TotalDamage ?? FixedPoint2.Zero;
        // account for the damage that the other entity has already taken: don't overkill
        threshold -= otherEntDamage;

        // The max amount of damage our meteor can take before breaking.
        var maxMeteorDamage = _destructible.DestroyedAt(uid) - CompOrNull<DamageableComponent>(uid)?.TotalDamage ?? FixedPoint2.Zero;

        // Cap damage so we don't overkill the meteor
        var trueDamage = FixedPoint2.Min(maxMeteorDamage, threshold);

        var damage = component.DamageTypes * trueDamage;
        _damageable.TryChangeDamage(args.Target, damage, true, origin: uid);
        _damageable.TryChangeDamage(uid, damage);

        if (!TerminatingOrDeleted(args.Target))
            component.HitList.Add(args.Target);
    }
}
