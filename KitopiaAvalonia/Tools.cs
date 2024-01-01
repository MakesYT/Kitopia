using System;

namespace KitopiaAvalonia;

public class Tools
{
    public static void GetAllEventHandle()
    {
        var i = 0b11;
        Console.WriteLine($"Ctrl {(i & (1 << 3)) != 0}");
        Console.WriteLine($"Shift {(i & (1 << 2)) != 0}");
        Console.WriteLine($"Alt {(i & (1 << 1)) != 0}");
        Console.WriteLine($"Win {(i & (1)) != 0}");
        Console.WriteLine($"None {(i) == 0}");
        Console.WriteLine($"KeyName {(i) != 0}");
    }
}