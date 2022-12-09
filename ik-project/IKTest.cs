using Godot;
using System;

public class IK : Godot.Node2D
{
	[Export]
	private bool flipped = false;
	[Export]
	private int calfLen = 100;
	[Export]
	private int thighLen = 120;

	private Vector2 kneePos = new Vector2();
	private Vector2 footPos = new Vector2();
	private float hipFootAngle = 0;
	private float hipFootDist = 0;
	private float thighAngle = 0;


	public override void _Ready()
	{
		GD.Print("Hello!");
	}

	public override void _PhysicsProcess(float delta) 
	{
		var mousePos = GetViewport().GetMousePosition();
		Position = mousePos;
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
