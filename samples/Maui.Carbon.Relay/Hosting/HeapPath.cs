using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Maui.Carbon.Relay.Hosting;

/// <summary>
/// Exercise mode: finds what keeps a leaked object alive. Walks the managed heap breadth first from every static field of
/// every loaded type (which covers the application, its services and the runtime's own tables) through instance fields,
/// arrays and delegates, and returns the shortest chain of references that reaches the target. Weak references and
/// GC handles are not followed, so "no managed path" means the object is held natively (a platform view's handle) or only
/// from a running method's locals.
/// </summary>
public static class HeapPath
{
    const int MaxObjects = 4_000_000;

    static readonly Dictionary<Type, FieldInfo[]> s_fields = [];

    public static string Find(object target)
    {
        var parents = new Dictionary<object, (object? Parent, string Edge)>(ReferenceEqualityComparer.Instance);
        var queue = new Queue<object>();

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types.Where(t => t is not null).ToArray()!;
            }
            foreach (var type in types)
            {
                if (type.ContainsGenericParameters) continue;
                FieldInfo[] statics;
                try
                {
                    statics = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                }
                catch
                {
                    continue;
                }
                foreach (var field in statics)
                {
                    if (field.IsLiteral || !Followed(field.FieldType)) continue;
                    object? value;
                    try
                    {
                        value = field.GetValue(null);
                    }
                    catch
                    {
                        continue;
                    }
                    if (value is null || parents.ContainsKey(value)) continue;
                    parents[value] = (null, $"{type.FullName}.{field.Name}");
                    queue.Enqueue(value);
                }
            }
        }

        AddNativeRoots(parents, queue);

        while (queue.Count > 0 && parents.Count < MaxObjects)
        {
            var current = queue.Dequeue();
            if (ReferenceEquals(current, target)) return Describe(target, parents);
            foreach (var (child, edge) in Children(current))
            {
                if (child is null || parents.ContainsKey(child)) continue;
                parents[child] = (current, edge);
                queue.Enqueue(child);
            }
        }
        return parents.Count >= MaxObjects
            ? $"gave up after {MaxObjects:N0} objects"
            : $"no path from a static field or a natively retained peer ({parents.Count:N0} objects walked)";
    }

    /// <summary>
    /// Objective-C peers that native code retains (more than the managed peer's own reference): the runtime keeps these
    /// alive for as long as the native object lives, so each one is a root (the runtime's own map holds them only weakly).
    /// </summary>
    static void AddNativeRoots(Dictionary<object, (object? Parent, string Edge)> parents, Queue<object> queue)
    {
#if IOS || MACCATALYST
        var field = typeof(ObjCRuntime.Runtime).GetField("object_map", BindingFlags.Static | BindingFlags.NonPublic);
        if (field?.GetValue(null) is not Dictionary<nint, System.Runtime.InteropServices.GCHandle> map) return;
        List<System.Runtime.InteropServices.GCHandle> handles;
        lock (map) handles = [.. map.Values];
        foreach (var handle in handles)
        {
            if (!handle.IsAllocated || handle.Target is not Foundation.NSObject peer || peer.Handle == IntPtr.Zero) continue;
            var retain = peer.RetainCount;
            if (retain < 2 || parents.ContainsKey(peer)) continue;
            parents[peer] = (null, $"natively retained {peer.GetType().Name} (retain count {retain}{NativeDetail(peer)})");
            queue.Enqueue(peer);
        }
#endif
    }

#if IOS || MACCATALYST
    [System.Runtime.InteropServices.DllImport("/usr/lib/libobjc.dylib")]
    static extern IntPtr sel_registerName(string name);

    [System.Runtime.InteropServices.DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    static extern IntPtr Send(IntPtr receiver, IntPtr selector);

    [System.Runtime.InteropServices.DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    static extern IntPtr SendIndex(IntPtr receiver, IntPtr selector, nuint index);

    [System.Runtime.InteropServices.DllImport("/usr/lib/libobjc.dylib")]
    static extern IntPtr object_getClassName(IntPtr obj);

    static string ClassOf(IntPtr o) => System.Runtime.InteropServices.Marshal.PtrToStringUTF8(object_getClassName(o)) ?? "?";

    /// <summary>
    /// For a view: its native superview chain (class and retain count, read through the Objective-C runtime, since the
    /// managed peers of those views may be gone), whether it is in a window, its gesture recognizers, and the MAUI view it draws.
    /// </summary>
    static string NativeDetail(Foundation.NSObject peer)
    {
        if (peer is not UIKit.UIView) return "";
        IntPtr superview = sel_registerName("superview"), window = sel_registerName("window"), retainCount = sel_registerName("retainCount"),
            gestures = sel_registerName("gestureRecognizers"), count = sel_registerName("count"), at = sel_registerName("objectAtIndex:");
        var chain = new List<string>();
        for (var v = Send(peer.Handle, superview); v != IntPtr.Zero && chain.Count < 12; v = Send(v, superview))
            chain.Add($"{ClassOf(v)}({(long)Send(v, retainCount)})");
        var list = Send(peer.Handle, gestures);
        var recognizers = new List<string>();
        if (list != IntPtr.Zero)
            for (nuint i = 0; i < (nuint)(long)Send(list, count); i++)
            {
                var g = SendIndex(list, at, i);
                recognizers.Add($"{ClassOf(g)}({(long)Send(g, retainCount)})");
            }
        var maui = "";
        if (peer is Microsoft.Maui.Platform.PlatformTouchGraphicsView
            && peer.GetType().GetField("_drawable", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(peer) is Element e)
        {
            var parents = new List<string>();
            for (Element? x = e; x is not null && parents.Count < 8; x = x.Parent) parents.Add(x.GetType().Name);
            maui = $"; draws {string.Join(" < ", parents)}";
        }
        return $"; window: {(Send(peer.Handle, window) == IntPtr.Zero ? "none" : "yes")}; superviews: {(chain.Count == 0 ? "none" : string.Join(" < ", chain))}; gestures: {(recognizers.Count == 0 ? "none" : string.Join(", ", recognizers))}{maui}";
    }
#endif

    static IEnumerable<(object? Child, string Edge)> Children(object o)
    {
        var type = o.GetType();
        if (type.IsArray)
        {
            if (!Followed(type.GetElementType()!)) yield break;
            var i = 0;
            foreach (var item in (IEnumerable)o) yield return (item, $"[{i++}]");
            yield break;
        }
        if (IsWeak(type)) yield break;
        foreach (var field in FieldsOf(type))
        {
            object? value;
            try
            {
                value = field.GetValue(o);
            }
            catch
            {
                continue;
            }
            yield return (value, field.Name);
        }
    }

    static FieldInfo[] FieldsOf(Type type)
    {
        if (s_fields.TryGetValue(type, out var cached)) return cached;
        var list = new List<FieldInfo>();
        for (var t = type; t is not null; t = t.BaseType)
            foreach (var f in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                if (Followed(f.FieldType)) list.Add(f);
        return s_fields[type] = [.. list];
    }

    /// <summary>Whether a value of this type can lead to other objects.</summary>
    static bool Followed(Type t) =>
        !(t.IsPrimitive || t.IsEnum || t.IsPointer || t == typeof(string) || t == typeof(IntPtr) || t == typeof(UIntPtr)
          || t == typeof(decimal) || t == typeof(DateTime) || t == typeof(TimeSpan) || t == typeof(Guid)
          || typeof(MemberInfo).IsAssignableFrom(t) || t.IsByRefLike);

    static bool IsWeak(Type t) =>
        t == typeof(WeakReference) || t.IsGenericType && t.GetGenericTypeDefinition() == typeof(WeakReference<>)
        || t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ConditionalWeakTable<,>)
        || t.FullName?.StartsWith("System.Reflection.", StringComparison.Ordinal) == true
        || t.FullName == "System.RuntimeType";

    static string Describe(object target, Dictionary<object, (object? Parent, string Edge)> parents)
    {
        var chain = new List<string>();
        for (object? o = target; o is not null; o = parents[o].Parent)
        {
            var (parent, edge) = parents[o];
            chain.Add(parent is null ? $"{edge} ({Name(o)})" : $".{edge} -> {Name(o)}");
        }
        chain.Reverse();
        var sb = new StringBuilder();
        foreach (var step in chain) sb.Append(sb.Length == 0 ? "" : "\n    ").Append(step);
        return sb.ToString();
    }

    static string Name(object o)
    {
        var t = o.GetType();
        var tick = t.Name.IndexOf('`');
        var name = t.IsGenericType ? $"{(tick < 0 ? t.Name : t.Name[..tick])}<{string.Join(",", t.GetGenericArguments().Select(a => a.Name))}>" : t.Name;
        if (t.DeclaringType is { } outer) name = $"{outer.Name}+{name}";
        return o is Delegate d ? $"{name} ({d.Method.DeclaringType?.Name}.{d.Method.Name})" : name;
    }
}
