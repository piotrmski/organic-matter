using System;

namespace Organicmatter.Scripts.Internal.Model
{
    [Flags]
    internal enum Direction
    {
        None = 0,
        Left = 1 << 0,
        Right = 1 << 1,
        Bottom = 1 << 2,
        Top = 1 << 3
    }
}
