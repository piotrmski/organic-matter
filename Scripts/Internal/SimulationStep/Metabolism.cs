using Organicmatter.Scripts.Internal.Model;
using System.Threading.Tasks;

namespace Organicmatter.Scripts.Internal.SimulationStep
{
    internal class Metabolism : ISimulationStep
    {
        private int _spaceWidth;

        private int _spaceHeight;

        private int _xMax;

        private int _yMax;

        private SimulationState _simulationState;

        public Metabolism(SimulationState simulationState)
        {
            _simulationState = simulationState;

            _spaceWidth = simulationState.CellMatrix.GetLength(0);

            _spaceHeight = simulationState.CellMatrix.GetLength(1);

            _xMax = _spaceWidth - 1;

            _yMax = _spaceHeight - 1;
        }

        public void Advance()
        {
            Parallel.For(0, _spaceWidth, x =>
            {
                for (int y = 0; y < _spaceHeight; y++)
                {
                    ref CellData cell = ref _simulationState.CellMatrix[x, y];

                    IncrementCounters(ref cell);

                    if (cell.Type == CellType.Soil)
                    {
                        ConvertEnergyAndWasteToNutrients(ref cell);
                        continue;
                    }

                    if (!cell.IsPlant()) { continue; }

                    if (cell.Type == CellType.PlantGreen)
                    {
                        TakeInLightEnergy(ref cell, x, y);

                        Photosynthesize(ref cell);
                    }

                    ConsumeEnergyOrDie(ref cell, x, y);
                }
            });
        }

        private void ConvertEnergyAndWasteToNutrients(ref CellData cell)
        {
            if ((cell.TicksSinceSynthesis % _simulationState.Parameters.SoilNutrientReclamationPeriod) > 0) { return; }

            if (cell.EnergyContent > 0)
            {
                cell.NutrientContent += 1;
                cell.EnergyContent -= 1;
                return;
            }

            if (cell.WasteContent > 0)
            {
                cell.NutrientContent += 1;
                cell.WasteContent -= 1;
                return;
            }
        }

        private void TakeInLightEnergy(ref CellData cell, int x, int y)
        {
            if (x == 0) { cell.AccumulatedLightEnergy += _simulationState.Parameters.DirectLightEnergy; }
            else if (_simulationState.CellMatrix[x - 1, y].Type == CellType.Air) { cell.AccumulatedLightEnergy += _simulationState.CellMatrix[x - 1, y].LightEnergy; }

            if (x == _xMax) { cell.AccumulatedLightEnergy += _simulationState.Parameters.DirectLightEnergy; }
            else if (_simulationState.CellMatrix[x + 1, y].Type == CellType.Air) { cell.AccumulatedLightEnergy += _simulationState.CellMatrix[x + 1, y].LightEnergy; }

            if (y > 0 && _simulationState.CellMatrix[x, y - 1].Type == CellType.Air) { cell.AccumulatedLightEnergy += _simulationState.CellMatrix[x, y - 1].LightEnergy; }

            if (y == _yMax) { cell.AccumulatedLightEnergy += _simulationState.Parameters.DirectLightEnergy; }
            else if (_simulationState.CellMatrix[x, y + 1].Type == CellType.Air) { cell.AccumulatedLightEnergy += _simulationState.CellMatrix[x, y + 1].LightEnergy; }

            if (cell.AccumulatedLightEnergy > _simulationState.Parameters.LightToConvertNutrientToEnergy) { cell.AccumulatedLightEnergy = _simulationState.Parameters.LightToConvertNutrientToEnergy; }
        }

        private void Photosynthesize(ref CellData cell)
        {
            if (!AreConditionsMetForPhotosynthesis(cell)) { return; }

            cell.EnergyContent += 1;
            cell.AccumulatedLightEnergy -= _simulationState.Parameters.LightToConvertNutrientToEnergy;
            cell.NutrientContent -= 1;

            cell.TicksSinceLastPhotosynthesis = 0;
        }

        private bool AreConditionsMetForPhotosynthesis(CellData cell)
        {
            return cell.AccumulatedLightEnergy >= _simulationState.Parameters.LightToConvertNutrientToEnergy &&
                cell.NutrientContent >= 1;
        }

        private void ConsumeEnergyOrDie(ref CellData cell, int x, int y)
        {
            int energyToConsume = cell.TicksSinceSynthesis > 0 &&
                ((cell.TicksSinceSynthesis % _simulationState.Parameters.PlantEnergyConsumptionPeriod) == 0) ? 1 : 0;

            if (cell.EnergyContent < energyToConsume || cell.WasteContent >= _simulationState.Parameters.WasteToKillPlantCell)
            {
                cell.Type = CellType.Water;
                cell.NutrientContent += _simulationState.Parameters.EnergyInPlantCellStructure;

                _simulationState.RemoveCellConnections(x, y);
            }
            else
            {
                cell.EnergyContent -= energyToConsume;
                cell.WasteContent += energyToConsume;
            }
        }

        private static void IncrementCounters(ref CellData cell)
        {
            if (cell.TicksSinceLastPhotosynthesis < int.MaxValue)
            {
                cell.TicksSinceLastPhotosynthesis += 1;
            }
            else
            {
                cell.TicksSinceLastPhotosynthesis /= 2;
            }

            if (cell.TicksSinceSynthesis < int.MaxValue)
            {
                cell.TicksSinceSynthesis += 1;
            }
            else
            {
                cell.TicksSinceSynthesis /= 2;
            }
        }
    }
}
