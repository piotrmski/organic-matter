namespace Organicmatter.Scripts.Internal.Model
{
    internal struct CellData
    {
        public CellType Type;

        public int LightEnergy;

        public int CapturedLightEnergy;

        public int WaterMolecules;

        public int SugarMolecules;

        public int AtpMolecules;

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
            string result = Type.ToString();

            if (Type == CellType.Air)
            {
                result += $"\nLight energy = {LightEnergy}";
            }

            if (CanDiffuse())
            {
                result += $"\nWater molecules = {WaterMolecules}";
            }

            return result;
        }
    }
}
