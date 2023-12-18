namespace Organicmatter.Scripts.Internal.Model
{
    internal struct SimulationParameters
    {
        public int MineralsCriticalSoilDistribution = 50;

        public int DirectLightEnergy = 16;

        public int LightToConvertMineralToEnergy = 1000;

        public int EnergyToSynthesizePlantCell = 2;

        public int WasteToKillPlantCell = 50;

        public SimulationParameters() { }
    }
}
