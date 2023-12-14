using Godot;
using Organicmatter.Scripts.Internal;
using Organicmatter.Scripts.Internal.Model;
using Organicmatter.Scripts.Internal.RenderingStrategy;
using System;
using System.Linq;

public partial class SimViewport : TextureRect
{
	[Export]
	private int _spaceWidth = 100;

	[Export]
	private int _spaceHeight = 100;

	[Export]
	private Label _debugLabel1;

	[Export]
	private Label _debugLabel2;

	[Export]
	private ItemList _renderModeList;

	[Export]
	private ItemList _simulationSpeedList;

	private IRenderer _renderer;

	private ImageTexture _viewportTexture = new();

	private Simulation _simulation;

	private Vector2I? _hoveredCell;

	private System.Diagnostics.Stopwatch watch = new();

	public override void _Ready()
	{
		_simulation = new Simulation(_spaceWidth, _spaceHeight);

		_renderModeList.Select(0);
		
		UpdateRendererByListIndex(0);

		Texture = _viewportTexture;

		_renderModeList.ItemSelected += UpdateRendererByListIndex;

		_simulationSpeedList.Select(0);
	}

	public override void _PhysicsProcess(double delta)
	{
		watch.Restart();
		AdvanceSimulationSelectedNumberOfTimes();
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

	private void AdvanceSimulationSelectedNumberOfTimes()
	{
		int multiplier = _simulationSpeedList.GetSelectedItems().FirstOrDefault() switch
		{
			0 => 1,
			1 => 2,
			2 => 5,
			3 => 10,
			4 => 20,
			5 => 50,
			_ => 0
		};

		for (int i = 0; i < multiplier; i++) _simulation.Advance();
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

	private IRenderer GetRendererByListIndex(long listIndex)
	{
		return listIndex switch
		{
			0 => new DefaultRenderer(_simulation.SimulationState),
			1 => new WaterRenderer(_simulation.SimulationState),
			2 => new GlucoseRenderer(_simulation.SimulationState),
			3 => new AtpRenderer(_simulation.SimulationState),
			4 => new PhotosynthesisRenderer(_simulation.SimulationState),
			_ => new RespirationRenderer(_simulation.SimulationState),
		};
	}

	private void UpdateRendererByListIndex(long listIndex)
	{
		_renderer = GetRendererByListIndex(listIndex);

		_viewportTexture.SetImage(_renderer.RenderedImage);
	}
}
