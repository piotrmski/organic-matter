using Godot;
using Organicmatter.Scripts.Internal;
using Organicmatter.Scripts.Internal.Helpers;
using Organicmatter.Scripts.Internal.Model;

public partial class SimViewport : TextureRect
{
	[Export]
	private int _spaceWidth = 100;

	[Export]
	private int _spaceHeight = 100;

	[Export]
	private double _simulationStepLength = .05;

	[Export]
	private Label _debugLabel1;

	[Export]
	private Label _debugLabel2;

	private Renderer _renderer;

	private ImageTexture _viewportTexture = new();

	private Simulation _simulation;

	private double _timeSinceLastSimulationStep = 0;

	private Vector2I? _hoveredCell;

	private System.Diagnostics.Stopwatch watch = new();

	public override void _Ready()
	{
		_simulation = new Simulation(_spaceWidth, _spaceHeight);

		_renderer = new Renderer(_simulation.SimulationState);

		_viewportTexture.SetImage(_renderer.RenderedImage);

		Texture = _viewportTexture;
	}

	public override void _PhysicsProcess(double delta)
	{
		_timeSinceLastSimulationStep += delta;

		if (_timeSinceLastSimulationStep < _simulationStepLength) { return; }

		_timeSinceLastSimulationStep -= _simulationStepLength;

		watch.Restart();
		_simulation.Advance();
		watch.Stop();

		if (_debugLabel2 != null) _debugLabel2.Text = $"{watch.ElapsedMilliseconds} ms";

		_renderer.UpdateImage();

		_viewportTexture.Update(_renderer.RenderedImage);

		UpdateHoveredCellInfo();
	}

	public override void _Input(InputEvent @event)
	{
		switch (@event)
		{
			case InputEventMouseMotion mouseMotionEvent:
				if (_debugLabel1 == null) { return; }

				int x = (int)((mouseMotionEvent.Position.X - Position.X) * _spaceWidth / Size.X);
				int y = (int)((Size.Y - mouseMotionEvent.Position.Y - Position.Y) * _spaceHeight / Size.Y);

				if (x < 0 || x >= _spaceWidth || y < 0 || y >= _spaceHeight)
				{
					_hoveredCell = null;
				}
				else
				{
					_hoveredCell = new Vector2I(x, y);
				}

				UpdateHoveredCellInfo();

				return;
		}
	}

	private void UpdateHoveredCellInfo()
	{
		if (_debugLabel1 == null) { return; }

		int waterSum = 0;
		int glucoseSum = 0;
		int celluloseSum = 0;

		_simulation.SimulationState.ForEachCell((ref CellData cell) =>
		{
			waterSum += cell.WaterMolecules;
			glucoseSum += cell.GlucoseMolecules;
			celluloseSum += cell.IsPlant() || cell.Type == CellType.Soil ? 1 : 0;
		});

		int carbonAtoms = _simulation.SimulationState.CarbonDioxydeMolecules +
			glucoseSum * 6 +
			celluloseSum * 6 * _simulation.SimulationState.Parameters.GlucoseInCellulose;

		int oxygenAtoms = _simulation.SimulationState.OxygenMolecules * 2 +
			_simulation.SimulationState.CarbonDioxydeMolecules * 2 +
			waterSum +
			glucoseSum * 6 +
			celluloseSum * 5 * _simulation.SimulationState.Parameters.GlucoseInCellulose;

		int hydrogenAtoms = waterSum * 2 +
			glucoseSum * 12 +
			celluloseSum * 12 * _simulation.SimulationState.Parameters.GlucoseInCellulose;

		_debugLabel1.Text = $"Total water molecules = {waterSum}\n" +
			$"Total glucose molecules = {glucoseSum}\n" +
			$"Total cellulose cells = {celluloseSum}\n\n" +
			$"Total carbon atoms = {carbonAtoms}\n" +
			$"Total oxygen atoms = {oxygenAtoms}\n" +
			$"Total hydrogen atoms = {hydrogenAtoms}\n\n" +
			$"CO2 molecules in the atmosphere = {_simulation.SimulationState.CarbonDioxydeMolecules}\n" +
			$"O2 molecules in the atmosphere = {_simulation.SimulationState.OxygenMolecules}\n\n";


		if (_hoveredCell != null)
		{
			_debugLabel1.Text += $"X = {_hoveredCell.Value.X} Y = {_hoveredCell.Value.Y}\n" +
				$"{_simulation.SimulationState.CellMatrix[_hoveredCell.Value.X, _hoveredCell.Value.Y]}";
		}
	}
}
