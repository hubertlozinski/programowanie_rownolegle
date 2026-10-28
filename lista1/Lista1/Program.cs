using System;
using System.Threading;
using System.Collections.Generic;

static class Program
{
    const int ORE_SIZE = 2000;
    const int TRUCK_LOAD = 200;
    const int EXTRACTION_TIME_MS = 10;
    const int UNLOAD_TIME = 10;
    const int TRANSPORT_TIME = 10000;

    static int ore = ORE_SIZE;
    static int warehouse = 0;
    static readonly object mtxStan = new object(); //chroni ore i warehouse
    static readonly object mtxLog = new object();
    static readonly string[] minerStatus = new string[16];
    static int minersCount = 0;
    const int PANEL_ROW = 0;

    static SemaphoreSlim semZloze = new SemaphoreSlim(2);
    static SemaphoreSlim semMagazyn = new SemaphoreSlim(1);

    static int time_divider = 1;

    //sim delay time divider
    static void simDelay(int ms)
    {
        Thread.Sleep(Math.Max(1, ms / time_divider));
    }

    static void log(string s)
    {
        lock (mtxLog)
        {
            Console.WriteLine(s);
        }
    }

    static void miner(int id)
    {
        while (true)
        {
            int load = 0;
            int left = 0;
            terminalStatus(id, $"waitin");
            semZloze.Wait();
            while (load < TRUCK_LOAD)
            {
                lock (mtxStan)
                {
                    if (ore == 0) break;
                    --ore;
                    ++load;
                    left = ore;
                }
                simDelay(EXTRACTION_TIME_MS);
            }
            semZloze.Release();

            if (load == 0) break; // ore is out
            lock (mtxStan)
            {
                left = ore;
            }
            /*log("Miner" + id + " extracted: " + load + " coal units. Left: " + left);
            log("Miner" + id + " transporting");*/
            terminalStatus(id, $"{load}, {left}");
            terminalStatus(id, $"transport");

            semMagazyn.Wait();
            terminalStatus(id, $"unloading ");
            /*log("Miner" + id + " unloading");*/
            for (int i = 0; i < load; ++i)
            {
                simDelay(UNLOAD_TIME);
                lock (mtxStan)
                {
                    ++warehouse;
                }
            }
            semMagazyn.Release();

            // return do ore
            bool this_is_the_end;
            lock (mtxStan)
            {
                this_is_the_end = (ore == 0);
            }
            if (this_is_the_end) break;
            terminalStatus(id, "returns to ore diggin");
            simDelay(TRANSPORT_TIME);
        }
    }

    static void terminalStatus(int id, string text)
    {
        lock (mtxLog)
        {
            minerStatus[id] = text;
            int row = PANEL_ROW + id + 1;
            Console.SetCursorPosition(0, row);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, row);
            Console.Write($"Miner{id}: {text}");

            int oreUpdate, warehouseUpdate;
            lock (mtxStan);
            {
                oreUpdate = ore;
                warehouseUpdate = warehouse;
            }
            int oreRow = PANEL_ROW + minersCount + 1;
            Console.SetCursorPosition(0, oreRow);
            Console.Write(new string(' ', Console.WindowWidth - 1));
            Console.SetCursorPosition(0, oreRow);
            Console.Write($"ORE: {oreUpdate}, STORED:{warehouseUpdate}");

            Console.SetCursorPosition(0, PANEL_ROW + minersCount + 3);
        }
    }

    static void Main(string[] args)
    {
        int miners_number = 5;

        if (args.Length > 0) miners_number = Math.Max(1, int.Parse(args[0]));
        if (args.Length > 1) time_divider = Math.Max(1, int.Parse(args[1]));

        Console.Clear();
        /*for (int i = 1 ; i <= miners_number; ++i)
        {
            Console.Write($"Miner{i}: czeka ");
        }*/
        Console.WriteLine();

        /*Console.WriteLine("Start: " + miners_number + " gornikow, zloze " + ORE_SIZE + ", pojemnosc pojazdu " + TRUCK_LOAD + " dzielnik czasu " + time_divider + "\n");
        Console.WriteLine($"Stan złoża: {ore} jednostek węgla".PadRight(50));8*/

        var start = DateTime.UtcNow;

        var threads = new List<Thread>();
        for (int i = 1; i <= miners_number; ++i)
        {
            int id = i;
            threads.Add(new Thread(() => miner(id)));
        }
        foreach (var t in threads) t.Start();
        foreach (var t in threads) t.Join();

        long czas = (long)(DateTime.UtcNow - start).TotalMilliseconds;

        /*Console.WriteLine("\nORE EXTRACTED AND STORED: " + warehouse + " / " + ORE_SIZE);
        Console.WriteLine("Calkowity czas przeniesienia wegla: " + (czas / 1000.0) + " s (liczbie gornikow: " + miners_number + ")");*/
    }
}