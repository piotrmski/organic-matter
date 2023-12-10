using System.Linq;
using Godot;
using Organicmatter.Scripts.Internal.Model;

namespace Organicmatter.Scripts.Internal.Helpers
{
    internal class AirInSoilSearch
    {
        private int _spaceWidth;

        private int _spaceHeight;

        private SimulationState _simulationState;

        private bool[,] _searchTargets;

        private AStar2D _searchAlgorithm = new();

        private int _searchDestinationId;

        public AirInSoilSearch(SimulationState simulationState)
        {
            _simulationState = simulationState;

            _spaceWidth = simulationState.CellMatrix.GetLength(0);

            _spaceHeight = simulationState.CellMatrix.GetLength(1);

            _searchTargets = new bool[_spaceWidth, _spaceHeight];

            _searchDestinationId = _spaceWidth * _spaceWidth;

            InitializeSearchAlgorithm();
        }

        public Vector2I? FindNearestAir(int fromX, int fromY) 
        {
            if (_simulationState.CellMatrix[fromX, fromY].Type != CellType.Soil) { return null; }

            DetermineSearchTargets();

            if (!_searchTargets.Cast<bool>().Any(x => x)) { return null; }

            PrepareSearchAlgorithmForCurrentSearch(fromX);

            long[] pathFound = _searchAlgorithm.GetIdPath(CoordinatesToIndex(fromX, fromY), _searchDestinationId);

            if(!(pathFound?.Length > 2)) { return null; }

            return IndexToCoordinates((int)pathFound[pathFound.Length - 2]);
        }

        private void DetermineSearchTargets()
        {
            _simulationState.ForEachCell((x, y) => _searchTargets[x, y] = false);

            _simulationState.ForEachCell((ref CellData cell, int x, int y) =>
            {
                if (cell.Type != CellType.Soil) { return; }

                if (IsAir(x - 1, y)) { _searchTargets[x - 1, y] = true; }
                if (IsAir(x + 1, y)) { _searchTargets[x + 1, y] = true; }
                if (IsAir(x, y + 1)) { _searchTargets[x, y + 1] = true; }
                // Downwards check intentionally omitted
            });
        }

        private bool IsAir(int x, int y)
        {
            return x > 0 && x < _spaceWidth && y < _spaceHeight && _simulationState.CellMatrix[x, y].Type == CellType.Air; // Downwards check intentionally omitted
        }

        private int CoordinatesToIndex(int x, int y)
        {
            return x * _spaceHeight + y;
        }

        private Vector2I IndexToCoordinates(int index)
        {
            return new Vector2I(index / _spaceHeight, index % _spaceHeight);
        }

        private void InitializeSearchAlgorithm()
        {
            _searchAlgorithm.ReserveSpace(_spaceWidth * _spaceWidth + 1);

            _simulationState.ForEachCell((x, y) => _searchAlgorithm.AddPoint(CoordinatesToIndex(x, y), new(x, y)));

            _simulationState.ForEachCell((x, y) =>
            {
                if (x < _spaceWidth - 1) _searchAlgorithm.ConnectPoints(CoordinatesToIndex(x, y), CoordinatesToIndex(x + 1, y));
                if (y < _spaceHeight - 1) _searchAlgorithm.ConnectPoints(CoordinatesToIndex(x, y), CoordinatesToIndex(x, y + 1));
            });

            _searchAlgorithm.AddPoint(_searchDestinationId, new());
        }

        private void PrepareSearchAlgorithmForCurrentSearch(int fromX)
        {
            _searchAlgorithm.RemovePoint(_searchDestinationId);

            _searchAlgorithm.AddPoint(_searchDestinationId, new(fromX, _spaceWidth * _spaceWidth * 10));

            _simulationState.ForEachCell((ref CellData cell, int x, int y) => 
            {
                bool isCellTraversibleForSearch = cell.Type == CellType.Soil || _searchTargets[x, y];

                _searchAlgorithm.SetPointDisabled(CoordinatesToIndex(x, y), !isCellTraversibleForSearch);

                if (_searchTargets[x, y])
                {
                    _searchAlgorithm.ConnectPoints(CoordinatesToIndex(x, y), _searchDestinationId);
                }
            });
        }
    }
}
