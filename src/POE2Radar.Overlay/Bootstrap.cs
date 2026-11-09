using POE2Radar.Core;
using POE2Radar.Core.Game;

namespace POE2Radar.Overlay;

/// <summary>
/// Resolves the PoE2 GameState global-pointer slot via the "Game States" AOB pattern, validated
/// by confirming the full chain resolves to a real local player. Returns the slot address (the
/// thing the RIP-relative instruction points at); deref it each tick to get the live GameState.
/// </summary>
internal static class Bootstrap
{
    public static IReadOnlyList<nint> FindCandidateGameStateSlots(ProcessHandle process, MemoryReader reader)
    {
        if (AobPatterns.GameStateRefs.Length == 0)
        {
            Console.Error.WriteLine("No GameState AOB patterns committed.");
            return Array.Empty<nint>();
        }

        var slots = new List<nint>();
        foreach (var pattern in AobPatterns.GameStateRefs)
        {
            slots.AddRange(AobScanner.ScanForResolvedAddresses(process, reader, pattern));
        }

        return slots.Distinct().ToList();
    }

    public static nint TryFindActiveSlot(MemoryReader reader, IEnumerable<nint> candidateSlots, out nint localPlayer)
    {
        localPlayer = 0;
        foreach (var slot in candidateSlots)
        {
            var live = new Poe2Live(reader, slot);
            if (live.TryResolve(out _, out _, out localPlayer))
                return slot;
        }
        return 0;
    }

    public static nint ResolveGameStateSlot(ProcessHandle process, MemoryReader reader)
    {
        var candidates = FindCandidateGameStateSlots(process, reader);
        if (candidates.Count == 0)
            return 0;

        var slot = TryFindActiveSlot(reader, candidates, out var localPlayer);
        if (slot != 0)
        {
            Console.WriteLine($"  GameState slot: 0x{slot:X16}  (LocalPlayer 0x{localPlayer:X16})");
            return slot;
        }

        Console.Error.WriteLine("Pattern matched but no slot resolved to an in-game chain.");
        Console.Error.WriteLine("Make sure you're loaded into a zone (not at login / character select).");
        return 0;
    }
}
