using Godot;
using Organicmatter.Scripts.Internal.Helpers;
using Organicmatter.Scripts.Internal.Model;

namespace Organicmatter.Scripts.Internal.SimulationStep
{
    internal class Germination : ISimulationStep
    {
        private int _spaceWidth;

        private int _spaceHeight;

        private SimulationState _simulationState;

        private SpecialCoordinates _specialCoordinates;

        public Germination(SimulationState simulationState, SpecialCoordinates specialCoordinates)
        {
            _simulationState = simulationState;

            _specialCoordinates = specialCoordinates;

            _spaceWidth = simulationState.CellMatrix.GetLength(0);

            _spaceHeight = simulationState.CellMatrix.GetLength(1);
        }

        public void Advance()
        {
            _specialCoordinates.Seeds.Delete(coordinates =>
            {
                if (CanGerminate(coordinates))
                {
                    Germinate(coordinates);

                    return true;
                }

                return false;
            });
        }

        private bool CanGerminate(Vector2I coordinates)
        {
            CellData cell = _simulationState.CellMatrix[coordinates.X, coordinates.Y];

            return HasEnoughEnergyToGrowRoot(cell) && HasSoilUnderneath(coordinates) && CanDiplaceSoilFromUnderneath(coordinates);
        }

        private bool HasEnoughEnergyToGrowRoot(CellData cell)
        {
            return cell.EnergyContent > _simulationState.Parameters.EnergyInPlantCellStructure;
        }

        private bool HasSoilUnderneath(Vector2I coordinates)
        {
            return coordinates.Y > 0 && _simulationState.CellMatrix[coordinates.X, coordinates.Y - 1].Type == CellType.Soil;
        }

        private bool CanDiplaceSoilFromUnderneath(Vector2I coordinates)
        {
            if (coordinates.X > 0)
            {
                if (_simulationState.CellMatrix[coordinates.X - 1, coordinates.Y - 1].Type == CellType.Air ||
                    _simulationState.CellMatrix[coordinates.X - 1, coordinates.Y].Type == CellType.Air) { return true; }
            }

            if (coordinates.X < _spaceWidth - 1)
            {
                if (_simulationState.CellMatrix[coordinates.X + 1, coordinates.Y - 1].Type == CellType.Air ||
                    _simulationState.CellMatrix[coordinates.X + 1, coordinates.Y].Type == CellType.Air) { return true; }
            }

            return false;
        }

        private void Germinate(Vector2I coordinates)
        {
            DisplaceSoilFromUnderneath(coordinates);

            GrowRootUnderSeed(coordinates);

            ConvertSeedToGreen(coordinates);
        }

        private void DisplaceSoilFromUnderneath(Vector2I coordinates)
        {
            if (coordinates.X > 0 && _simulationState.CellMatrix[coordinates.X - 1, coordinates.Y - 1].Type == CellType.Air)
            {
                Swap(coordinates.X, coordinates.Y - 1, coordinates.X - 1, coordinates.Y - 1);
            }

            if (coordinates.X < _spaceWidth - 1 && _simulationState.CellMatrix[coordinates.X + 1, coordinates.Y - 1].Type == CellType.Air)
            {
                Swap(coordinates.X, coordinates.Y - 1, coordinates.X + 1, coordinates.Y - 1);
            }

            if (coordinates.X > 0 && _simulationState.CellMatrix[coordinates.X - 1, coordinates.Y].Type == CellType.Air)
            {
                Swap(coordinates.X, coordinates.Y - 1, coordinates.X - 1, coordinates.Y);
            }

            if (coordinates.X < _spaceWidth - 1 && _simulationState.CellMatrix[coordinates.X + 1, coordinates.Y].Type == CellType.Air)
            {
                Swap(coordinates.X, coordinates.Y - 1, coordinates.X + 1, coordinates.Y);
            }
        }

        private void GrowRootUnderSeed(Vector2I coordinates)
        {
            _simulationState.CellMatrix[coordinates.X, coordinates.Y - 1].Type = CellType.PlantRoot;
            _simulationState.CellMatrix[coordinates.X, coordinates.Y - 1].TicksSinceSynthesis = 0;
            _simulationState.CellMatrix[coordinates.X, coordinates.Y].EnergyContent -= _simulationState.Parameters.EnergyInPlantCellStructure;
            _simulationState.AddCellConnections(coordinates.X, coordinates.Y, Direction.Down);
        }

        private void ConvertSeedToGreen(Vector2I coordinates)
        {
            _simulationState.CellMatrix[coordinates.X, coordinates.Y].Type = CellType.PlantGreen;
            _simulationState.CellMatrix[coordinates.X, coordinates.Y].TicksSinceSynthesis = 0;
        }

        private void Swap(int x1, int y1, int x2, int y2)
        {
            (_simulationState.CellMatrix[x1, y1], _simulationState.CellMatrix[x2, y2]) =
                (_simulationState.CellMatrix[x2, y2], _simulationState.CellMatrix[x1, y1]);
        }
    }
}
