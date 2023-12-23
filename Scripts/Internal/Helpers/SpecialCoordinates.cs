using Organicmatter.Scripts.Internal.Model;

namespace Organicmatter.Scripts.Internal.Helpers
{
    internal class SpecialCoordinates
    {
        public CoordinatesList Seeds { get; private set; } = new();

        public CoordinatesList Fruits { get; private set; } = new();
    }
}
