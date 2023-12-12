namespace Organicmatter.Scripts.Internal.Model
{
    internal struct CellData
    {
        public CellType Type;

        public int LightEnergy;

        public int AccumulatedLightEnergy;

        public int WaterMolecules;

        public int SugarMolecules;

        public int AtpEnergy;

        public bool IsSolid()
        {
            return Type != CellType.Air;
        }

        public bool CanFall()
        {
            return Type == CellType.Soil || Type == CellType.Bacteria;
        }

        public bool CanDiffuse()
        {
            return Type == CellType.Soil || IsPlant();
        }

        public bool CanDiffuseTo(CellData otherCell, bool areCellsConnected)
        {
            return Type == CellType.Soil && otherCell.Type == CellType.Soil ||
                IsPlant() && otherCell.IsPlant() && areCellsConnected ||
                Type == CellType.Soil && otherCell.Type == CellType.PlantRoot && WaterMolecules > otherCell.WaterMolecules;
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
                result += $"Water molecules = {WaterMolecules}\n";
                result += $"Sugar molecules = {SugarMolecules}\n";
            }

            if (IsPlant())
            {
                result += $"Energy in ATP = {AtpEnergy}\n";
            }

            if (Type == CellType.PlantGreen)
            {
                result += $"Light energy accumulated = {LightEnergy}\n";
            }

            return result;
        }
    }
}
