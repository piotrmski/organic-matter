namespace Organicmatter.Scripts.Internal.Model
{
    internal struct SimulationParameters
    {
        public int MineralsCriticalSoilDistribution = 20;

        public int DirectLightEnergy = 16;

        public int LightToConvertMineralToEnergy = 100;

        public int EnergyToSynthesizePlantCell = 2;

        public SimulationParameters() { }
    }
}
