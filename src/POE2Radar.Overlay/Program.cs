using System.ComponentModel;
using System.Diagnostics;
using POE2Radar.Core;
using POE2Radar.Overlay;

Console.Title = "POE2Radar Overlay";
Console.WriteLine("POE2Radar — map/radar overlay");
Console.WriteLine("=============================");

const string SingleInstanceMutexName = @"Local\POE2Radar_Overlay_SingleInstance";
using var singleInstanceMutex = new Mutex(true, SingleInstanceMutexName, out var isOnlyInstance);
if (!isOnlyInstance)
{
    Console.WriteLine("\n[INFO] Another instance of POE2Radar is already running.");
    Console.WriteLine("Exiting duplicate instance to avoid overlay conflicts with the game.");
    Thread.Sleep(1500);
    return 0;
}

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

try
{
    ProcessHandle? process = null;
    Console.WriteLine("Looking for Path of Exile 2 process...");
    while (!cts.IsCancellationRequested && process is null)
    {
        try
        {
            process = ProcessHandle.AttachToPoE();
        }
        catch (Win32Exception ex)
        {
            Console.Error.WriteLine($"\n[ERROR] Failed to attach to PoE2: {ex.Message}");
            Console.Error.WriteLine("Please make sure to run POE2Radar AS ADMINISTRATOR!");
            PauseBeforeExit();
            return 1;
        }

        if (process is null)
        {
            Console.Write("\rPoE2 not detected yet. Waiting for game to start... (Ctrl+C to quit) ");
            Thread.Sleep(1000);
        }
    }

    if (process is null || cts.IsCancellationRequested)
        return 0;

    Console.WriteLine($"\nAttached to {process.ProcessName} (PID {process.ProcessId})");

    var reader = new MemoryReader(process);

    Console.WriteLine("Scanning executable for GameState pointers...");
    var candidateSlots = Bootstrap.FindCandidateGameStateSlots(process, reader);
    if (candidateSlots.Count == 0)
    {
        Console.Error.WriteLine("[ERROR] Could not find any GameState pointer matching the AOB pattern.");
        Console.Error.WriteLine("The game may have updated with incompatible binary patterns.");
        PauseBeforeExit();
        return 2;
    }

    Console.WriteLine($"Found {candidateSlots.Count} candidate slot(s). Waiting for player to enter a zone...");
    nint slot = 0;
    nint localPlayer = 0;

    while (!cts.IsCancellationRequested && slot == 0)
    {
        if (Process.GetProcessesByName(process.ProcessName).Length == 0)
        {
            Console.WriteLine("\nPoE2 process has closed.");
            PauseBeforeExit();
            return 1;
        }

        slot = Bootstrap.TryFindActiveSlot(reader, candidateSlots, out localPlayer);
        if (slot != 0) break;

        Console.Write("\rWaiting for character to load into an area (town / hideout / map)... ");
        Thread.Sleep(1000);
    }

    if (slot == 0 || cts.IsCancellationRequested)
        return 0;

    Console.WriteLine($"\nResolved GameState slot: 0x{slot:X16} (LocalPlayer: 0x{localPlayer:X16})");
    Console.WriteLine();
    Console.WriteLine("==================================================================");
    Console.WriteLine("Radar running. Open the in-game map to see terrain + entities.");
    Console.WriteLine("Atlas: open it in-game; rings are auto-positioned. F10 = inspect tile.");
    Console.WriteLine("Web Dashboard: http://localhost:7777 (or press F12 in-game)");
    Console.WriteLine("Hotkeys: F8 = toggle auto-flask, F9 = quit radar, F6/F7 = route/clear");
    Console.WriteLine("Ctrl+C or close window to exit.");
    Console.WriteLine("==================================================================");

    using var app = new RadarApp(process, reader, slot);
    cts.Token.Register(() => app.RequestShutdown());
    app.Run();

    Console.WriteLine("Overlay shut down cleanly.");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"\n[FATAL ERROR] An unexpected error occurred: {ex.Message}");
    Console.Error.WriteLine(ex.StackTrace);
    PauseBeforeExit();
    return 99;
}

static void PauseBeforeExit()
{
    if (!Console.IsInputRedirected)
    {
        Console.WriteLine("\nPress any key to close...");
        try { Console.ReadKey(true); } catch { }
    }
}
