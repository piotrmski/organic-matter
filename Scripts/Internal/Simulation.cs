using Godot;
using Organicmatter.Scripts.Internal.Model;
using Organicmatter.Scripts.Internal.SimulationStrategy;
using System;
using System.Collections.Generic;

namespace Organicmatter.Scripts.Internal
{
    internal class Simulation
    {
        public SimulationState SimulationState { get; private set; }

        private List<ISimulationStrategy> _strategies;

        public Simulation(int spaceWidth, int spaceHeight) 
        {
            SimulationState = new SimulationState(spaceWidth, spaceHeight);

            PrepareInitialState();

            _strategies = new List<ISimulationStrategy>()
            {
                new Gravity(SimulationState),
                new Diffusion(SimulationState),
                new PlantGrowth(SimulationState),
                new Lighting(SimulationState),
                //new PlantMetabolism(SimulationState),
                //new PlantSeating(SimulationState),
            };
        }

        public void Advance()
        {
            _strategies.ForEach(strategy => strategy.Advance());
        }

        private void PrepareInitialState()
        {
            //for (int x = 0; x < SimulationState.CellMatrix.GetLength(0); x++)
            //{
            //    for (int y = 0; y < SimulationState.CellMatrix.GetLength(1); y++)
            //    {
            //        SimulationState.CellMatrix[x, y].Type = (x - 50) * (x - 50) + (y - 50) * (y - 50) < 35 * 35 ? CellType.Soil : CellType.Air;
            //        SimulationState.CellMatrix[x, y].MoisturePpm = (x - 50) * (x - 50) + (y - 50) * (y - 50) < 25 * 25 ? 1000000 : 0;
            //    }
            //}

            SimulationState.ForEachCell((ref CellData cellData, int x, int y) =>
            {
                if (y < 30)
                {
                    cellData.Type = CellType.Soil;
                    cellData.WaterMolecules = SimulationState.Parameters.WaterMoleculesStartingDistribution;
                }
                else
                {
                    cellData.Type = CellType.Air;
                }
            });

            int x = SimulationState.CellMatrix.GetLength(0) / 2;

            SimulationState.CellMatrix[x, 28].Type = CellType.PlantRoot;
            SimulationState.CellMatrix[x, 29].Type = CellType.PlantRoot;
            SimulationState.CellMatrix[x, 30].Type = CellType.PlantGreen;
            SimulationState.CellMatrix[x, 31].Type = CellType.PlantGreen;
            SimulationState.CellMatrix[x, 32].Type = CellType.PlantGreen;
            SimulationState.CellMatrix[x - 1, 32].Type = CellType.PlantGreen;
            SimulationState.CellMatrix[x + 1, 32].Type = CellType.PlantGreen;
            SimulationState.CellMatrix[x, 33].Type = CellType.PlantGreen;

            SimulationState.AddCellConnections(x, 29, Direction.Top | Direction.Bottom);
            SimulationState.AddCellConnections(x, 30, Direction.Top);
            SimulationState.AddCellConnections(x, 32, Direction.Left | Direction.Right | Direction.Top | Direction.Bottom);
        }
    }
}
