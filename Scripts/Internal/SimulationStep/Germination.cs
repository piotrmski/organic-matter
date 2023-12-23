using Organicmatter.Scripts.Internal.Helpers;
using Organicmatter.Scripts.Internal.Model;

namespace Organicmatter.Scripts.Internal.SimulationStep
{
    internal class Germination : ISimulationStep
    {
        private int _spaceWidth;

        private int _spaceHeight;

        private SimulationState _simulationState;

        private SeedCoordinates _seedCoordinates;

        public Germination(SimulationState simulationState, SeedCoordinates seedCoordinates)
        {
            _simulationState = simulationState;

            _seedCoordinates = seedCoordinates;

            _spaceWidth = simulationState.CellMatrix.GetLength(0);

            _spaceHeight = simulationState.CellMatrix.GetLength(1);
        }

        public void Advance()
        {

        }
    }
}
