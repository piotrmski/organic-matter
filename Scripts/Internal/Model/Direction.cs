using System;

namespace Organicmatter.Scripts.Internal.Model
{
    [Flags]
    internal enum Direction
    {
        None = 0,
        Left = 1 << 0,
        Right = 1 << 1,
        Down = 1 << 2,
        Up = 1 << 3
    }
}
