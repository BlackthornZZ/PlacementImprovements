using Il2CppInterop.Runtime.Injection;
using Il2CppMonomiPark.SlimeRancher.Event;
using Il2CppMonomiPark.SlimeRancher.Event.Query;
using MelonLoader;
using Starlight.Storage;
using Starlight.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildImprovements.Injected;

[InjectIntoIL]
public class NudgeGameEventQueryComponent : GameEventQueryComponent
{
    public static readonly IGameEvent OnNudgedEvent = new GameEvent("Gadget Nudged", "Gadget Nudged (Dataaa)").Cast<IGameEvent>();

    public NudgeGameEventQueryComponent(IntPtr ptr) : base(ptr) { }
    public NudgeGameEventQueryComponent() : base(ClassInjector.DerivedConstructorPointer<NudgeGameEventQueryComponent>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }
    public override IGameEvent GetEvent()
    {
        return OnNudgedEvent;
    }
}
