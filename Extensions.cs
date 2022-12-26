using System;

namespace ChristmasClockController {
    public static class Extensions {

    public static void Deconstruct<T>(this T[] list, out T first, out T second) {
        if(list.Length != 2) throw new ArgumentException("List must have exactly two elements", nameof(list));
        first = list[0];
        second = list[1];
    }
}
}