using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Numerics;
using view;
using Overlay;
using System.Diagnostics.Contracts;
using System.CodeDom;

public class H1Z1: MemoryLib.MemoryRead
{
    const ulong Base = 0x140000000;
    public static ulong gameManager(IntPtr process)
    {
        const long GameSmartPtr = 0x46E0598;

        Console.ForegroundColor = ConsoleColor.Blue;
        ulong control = ReadInt<ulong>(process, Base + GameSmartPtr, 8) & ~1UL;
        Console.WriteLine($"Control : 0x{control}");
        ulong cgs = ReadInt<ulong>(process, control + 0x10, 8);
        Console.WriteLine($"cgame : 0x{cgs}");
        ulong gameManager = ReadInt<ulong>(process, cgs + 0x382A0, 8);
        Console.WriteLine($"Game Manager : 0x{gameManager}\n");
        Console.ForegroundColor = ConsoleColor.White;
        return gameManager;
    }

    public static void viewPlayerESP(Matrix4x4 mat, Box box, int width, int height)
    {
        box.AddRectangle(
            (int)(width/3.10),
            height/2,
            250,
            height/2 - 40
        );
    }

    public static void infiniteAmmo(IntPtr process)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        bool write = WriteMem(process, 0x142867AC9, [ 0x44, 0x89, 0x03 ], 3);
        Console.WriteLine($"Infinite ammo : {write}\n");
        Console.ForegroundColor = ConsoleColor.White;
    }

    public static Vector3 readMe(IntPtr process, ulong localPlayer)
    {
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        string PlayerName = ReadName(process, ReadInt<ulong>(process, localPlayer + 0x7C0, 8));
        //Console.WriteLine($"Ton nom: {PlayerName}");

        ulong healthBase = ReadInt<ulong>(process, localPlayer + 0x3DC8, 8);
        int health = ReadInt<int>(process, healthBase + 0xB0, 4);
        //Console.WriteLine($"Ta vie: {health / 100}%");

        ulong posOffset = ReadInt<ulong>(process, localPlayer + 0x300, 8);
        Vector3 pos = ReadInt<Vector3>(process, posOffset + 0x3A8, 12);
        Console.WriteLine($"Player pos : {pos}");
        Console.ResetColor();

        return pos;
    }
    public static List<Vector3> readPlayers(IntPtr process, ulong game)
    {
        List<Vector3> Players = new();

        int entCount = ReadInt<int>(process, game + 0x50, 4);
        //Console.WriteLine($"Nombre d'entités : {entCount}");
        
        int countPlayer = ReadInt<int>(process, 0x144774440UL, 4);
        //Console.WriteLine($"Joueurs de la partie : {countPlayer}");
        
        ulong localPlayer = ReadInt<ulong>(process, game + 0x780, 8);
        ulong entEntry = ReadInt<ulong>(process, localPlayer + 0x748, 8);
        readMe(process, localPlayer);

        if (countPlayer > 0 && countPlayer < 250 && entCount > 0)
        {
            for (short i = 0; i < entCount; i++)
            {
                int Type = ReadInt<int>(process, entEntry + 0x3F8, 4);
                if (Type != 270)
                    continue;

                //Console.WriteLine($"entité entry : {entEntry:X}");

                ulong healthBase = ReadInt<ulong>(process, entEntry + 0x3DC8, 8);
                if (healthBase == 0) continue;

                string PlayerName = ReadName(process, ReadInt<ulong>(process, entEntry + 0x7C0, 8));
                Console.WriteLine($"Nom du joueur: {PlayerName}");

                int health = ReadInt<int>(process, healthBase + 0xB0, 4);
                Console.WriteLine($"Vie du joueur: {health / 100}%");

                ulong posOffset = ReadInt<ulong>(process, entEntry + 0x300, 8);
                Vector3 pos = ReadInt<Vector3>(process, posOffset + 0x3A8, 12);
                Console.WriteLine($"Position : {pos}");
                Players.Add(pos);

                Console.WriteLine();
                entEntry = ReadInt<ulong>(process, entEntry + 0x748, 8);
            }
        }
        return Players;
    }

    public static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Veuillez notez la largeur et la longeur en arguments du programme programme.exe 1920 1080");
            return;
        }
        Process process = Process.GetProcessesByName("H1Z1")[0];
        if (process.Id <= 0)
            throw new Exception($"Échec de l'ouverture du processus h1z1. peut être le jeu n'est pas ouvert ?");
        IntPtr processHandle = OpenProcess(PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION, false, process.Id);
        if (processHandle == IntPtr.Zero)
            throw new Exception($"Échec de l'ouverture du processus h1z1. Erreur : {Marshal.GetLastWin32Error()}");
        Console.WriteLine($"Ouverture : {process.ProcessName}");
        Console.ForegroundColor = ConsoleColor.Cyan;
        int width = Int32.Parse(args[0]);
        int height = Int32.Parse(args[1]);
        Console.WriteLine($"Ecran du jeu {width}x{height}");
        Console.ResetColor();

        ulong game = gameManager(processHandle);
        Matrix4x4 vMatrix = ReadInt<Matrix4x4>(processHandle, Base + 0x5FF3F20, 64);
        Console.WriteLine(vMatrix.ToString());

        //infiniteAmmo(processHandle);

        Box box = null!;

        var uiThread = new Thread(() =>
        {
            box = new Box(width, height);
            Application.Run(box);
        });

        uiThread.SetApartmentState(ApartmentState.STA);
        uiThread.Start();

        while (box == null)
            Thread.Sleep(10);
        while (true)
        {
            List<Vector3> Players = readPlayers(processHandle, game);
            box.ClearRectangles();
            //viewPlayerESP(vMatrix, box, width, height);
            foreach (var player in Players)
            {
                Vector3 head = player + new Vector3(0f, 1.75f, 0f);
                Vector2? feet2D = viewMat.WorldToScreen(player, vMatrix, width, height);
                Vector2? head2D = viewMat.WorldToScreen(head, vMatrix, width, height);

                if (feet2D.HasValue && head2D.HasValue)
                {
                    float h = feet2D.Value.Y - head2D.Value.Y;
                    float w = h * 0.5f;

                    box.AddRectangle(
                        (int)(head2D.Value.X - w / 2),
                        (int)head2D.Value.Y,
                        (int)w,
                        (int)h
                    );

                    Console.
                    WriteLine($"Rectangle ajouté coordonné 2d headX : {head2D.Value.X - w / 2}, headY : {head2D.Value.Y}, Width : {w}, Height : {h}");
                }
            }

            Thread.Sleep(100);
        }

    }
}