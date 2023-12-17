namespace Organicmatter.Scripts.Internal.Model
{
    internal struct CellData
    {
        public CellType Type;

        public int LightEnergy;

        public int AccumulatedLightEnergy;

        public int MineralContent;

        public int EnergyContent;

        public int WasteContent;

        public int TicksSinceSynthesis;

        public int TicksSinceLastPhotosynthesis;

        public bool IsSolid()
        {
            return Type != CellType.Air;
        }

        public bool CanFall()
        {
            return Type == CellType.Soil || Type == CellType.Water;
        }

        public bool CanDiffuse()
        {
            return Type != CellType.Air;
        }

        public bool IsPlant()
        {
            return Type == CellType.PlantRoot || Type == CellType.PlantGreen;
        }

        public bool CanPlantSynthesizeHere()
        {
            return Type == CellType.Soil || Type == CellType.Air;
        }

        public override string ToString()
        {
            string result = $"Type = {Type}\n";

            if (Type == CellType.Air)
            {
                result += $"Light energy = {LightEnergy}\n";
            }

            if (CanDiffuse())
            {
                result += $"Mineral content = {MineralContent}\n";
            }

            if (IsPlant())
            {
                result += $"Energy content = {EnergyContent}\n";
                result += $"Waste content = {WasteContent}\n";
                result += $"Age = {TicksSinceSynthesis}\n";
            }

            if (Type == CellType.PlantGreen)
            {
                result += $"Light energy accumulated = {AccumulatedLightEnergy}\n";
            }

            return result;
        }
    }
}
