using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildImprovements;

// Various functions for dealing with the consequences of IL2CPP.
internal static class InteropStatics
{
    // This function is UNSAFE (!!) but allows you to cast anything to anything
    // Doesn't change the actual memory behind the operation. The compiler only now interprets that memory as TDest rather than TSource, like reinterpret_cast<T> in C++.
    // Mostly useful for downcasting to an interface that got cut by IL2CPP (eg, StaticGameEvent : ScriptableObject, IGameEvent (IL) -> StaticGameEvent : ScriptableObject (IL2CPP))
    public static unsafe TDest ReinterpretCast<TSource, TDest>(TSource source)
    {
        var sourceRef = __makeref(source);
        var dest = default(TDest);
        var destRef = __makeref(dest);
#pragma warning disable CS8500
        *(IntPtr*)&destRef = *(IntPtr*)&sourceRef;
#pragma warning restore CS8500
        return __refvalue(destRef, TDest);
    }
}
