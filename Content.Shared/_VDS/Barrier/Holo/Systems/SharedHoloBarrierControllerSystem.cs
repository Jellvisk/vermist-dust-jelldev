using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Content.Shared._VDS.Barrier.Holo;

public abstract partial class SharedHoloBarrierControllerSystem : EntitySystem
{

    public override void Initialize()
    {
        base.Initialize();

    }

    private void UpdateController(Entity<HoloBarrierControllerComponent> controller)
    {

    }


}
