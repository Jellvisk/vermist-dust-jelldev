using Content.Shared.Examine;
using Content.Shared.Inventory;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Reflect;

namespace Content.Shared._VDS.Weapons.Reflect;

public sealed class ChargedReflectSystem : EntitySystem
{
    [Dependency] private readonly SharedBatterySystem _battery = default!;

    private EntityQuery<ReflectComponent> _reflectQuery;
    private EntityQuery<BatteryComponent> _batteryQuery;

    public override void Initialize()
    {
        base.Initialize();

        _reflectQuery = GetEntityQuery<ReflectComponent>();
        _batteryQuery = GetEntityQuery<BatteryComponent>();

        Subs.SubscribeWithRelay<ChargedReflectComponent, ProjectileReflectAttemptEvent>(OnProjectileReflectAttempt, baseEvent: true);
        Subs.SubscribeWithRelay<ChargedReflectComponent, HitScanReflectAttemptEvent>(OnHitscanReflectAttemptEvent, baseEvent: true);

        SubscribeLocalEvent<ChargedReflectComponent, ExaminedEvent>(OnExamined);
    }

    private void OnProjectileReflectAttempt(Entity<ChargedReflectComponent> ent, ref ProjectileReflectAttemptEvent args)
    {
        TryUpdateReflectProb(ent);
        Log.Info("proj");
    }

    private void OnHitscanReflectAttemptEvent(Entity<ChargedReflectComponent> ent, ref HitScanReflectAttemptEvent args)
    {
        TryUpdateReflectProb(ent);
        Log.Info("hitscanned");
    }

    private void OnExamined(Entity<ChargedReflectComponent> ent, ref ExaminedEvent args)
    {
        TryUpdateReflectProb(ent);
        Log.Info("examined");
    }

    private void TryUpdateReflectProb(
        Entity<ChargedReflectComponent> ent,
        ReflectComponent? reflect = null,
        BatteryComponent? battery = null)
    {
        if (!_reflectQuery.Resolve(ent, ref reflect) || !_batteryQuery.Resolve(ent, ref battery))
            return;

        var priorProb = reflect.ReflectProb;

        reflect.ReflectProb = CalculateReflectProb(
            _battery.GetChargeLevel((ent.Owner, battery)),
            ent.Comp.ChargeProbScalar ?? 1f,
            ent.Comp.ReflectProbMin,
            ent.Comp.ReflectProbMax);

        // don't dirty if there is practically no difference.
        if (MathHelper.CloseTo(reflect.ReflectProb, priorProb, 0.005f))
            return;

        DirtyField(ent.Owner, reflect, nameof(ReflectComponent.ReflectProb));
    }

    private static float CalculateReflectProb(float chargeLevel, float scalar, float minClamp, float maxClamp)
    {
        return MathHelper.Clamp(chargeLevel * scalar, minClamp, maxClamp);
    }
}
