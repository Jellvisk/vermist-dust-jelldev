using System.Linq;
using Content.Shared._VDS.Barrier;
using Content.Shared._VDS.Barrier.Pulveris.Components;
using Content.Shared._VDS.Barrier.Pulveris.Systems;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Utility;

namespace Content.Client._VDS.Barrier.Pulveris.Systems;

public sealed class PulverisBarrierRelaySystem : SharedPulverisBarrierRelaySystem
{
    [Dependency] private readonly AppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly AnimationPlayerSystem _animationPlayer = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PulverisBarrierRelayComponent, AnimationCompletedEvent>(OnAnimationCompleted);
    }

    private void OnAnimationCompleted(Entity<PulverisBarrierRelayComponent> ent, ref AnimationCompletedEvent args)
    {

        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        if (!_sprite.LayerMapTryGet((ent, sprite), "connectedLayer", out var layer, false) ||
            !_sprite.LayerMapTryGet((ent, sprite), "north", out var animatedLayer, false))
        {
            return;
        }

        _sprite.LayerSetVisible((ent, sprite), layer, true);
        _sprite.LayerSetVisible((ent, sprite), animatedLayer, false);
    }
}
