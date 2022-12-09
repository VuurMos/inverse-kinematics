using Godot;
using System;

public class IKTest : Godot.Node2D
{
	// settings
	[Export]
	private int flipped = 1;
	[Export]
	private int limbOneLen = 100;
	[Export]
	private int limbTwoLen = 120;
	private int maxLen;
	[Export]
	private int minLen = 20;


	// working variables
	private Vector2 anchorPos = new Vector2();
	private Vector2 jointPos = new Vector2();
	private Vector2 targetPos = new Vector2();
	private float anchorToTargetAngle = 0;
	private float anchorTargetDist = 0;
	private float anchorAngle = 0;

	// visual indicators
	private Position2D anchorPosInd;
	private Position2D targetPosInd;
	private Position2D jointPosInd;
	private Line2D anchorTargetLine;
	private Line2D anchorJointLine;
	private Line2D jointTargetLine;

	public override void _Ready()
	{
		maxLen = limbOneLen + limbTwoLen;
		anchorPosInd = (Position2D) GetNode("AnchorPos");
		targetPosInd = (Position2D) GetNode("TargetPos");
		jointPosInd = (Position2D) GetNode("JointPos");
		anchorTargetLine = (Line2D) GetNode("AnchorTargetLine");
		anchorJointLine = (Line2D) GetNode("AnchorJointLine");
		jointTargetLine = (Line2D) GetNode("JointTargetLine");
	}

	public override void _PhysicsProcess(float delta) 
	{
		CalculateIK();
		UpdateIKVisuals();
	}

	private void CalculateIK()
	{
		// find the clamped target position from the anchor using the ideal target positon (mouse position)
		anchorToTargetAngle = anchorPos.AngleToPoint(GetGlobalMousePosition());
		anchorTargetDist = Mathf.Clamp(anchorPos.DistanceTo(GetGlobalMousePosition()), minLen, maxLen);
		float targetPosX = anchorTargetDist * -Mathf.Cos(anchorToTargetAngle);
		float targetPosY = anchorTargetDist * -Mathf.Sin(anchorToTargetAngle);
		targetPos = new Vector2(targetPosX, targetPosY);

		// law of cosines to find anchor angle
		float aSqr = limbOneLen * limbOneLen;
		float bSqr = limbTwoLen * limbTwoLen;
		float cSqr = anchorTargetDist * anchorTargetDist;
		float clampedLawOfCosines = Mathf.Min(1, Mathf.Max(-1, (bSqr + cSqr - aSqr)/(2 * limbTwoLen * anchorTargetDist)));
		anchorAngle = flipped * Mathf.Acos(clampedLawOfCosines);

		// joint position
		var jointPosX = (limbTwoLen * Mathf.Cos(anchorAngle - anchorToTargetAngle));
		var jointPosY = (limbTwoLen * Mathf.Sin(anchorAngle - anchorToTargetAngle));
		jointPos = new Vector2(-jointPosX, jointPosY);
	}

	private void UpdateIKVisuals()
	{
		// update position indicators
		targetPosInd.Position = targetPos;
		jointPosInd.Position = jointPos;

		// update lines
		anchorTargetLine.SetPointPosition(0, anchorPos);
		anchorTargetLine.SetPointPosition(1, targetPos);
		anchorJointLine.SetPointPosition(0, anchorPos);
		anchorJointLine.SetPointPosition(1, jointPos);
		jointTargetLine.SetPointPosition(0, jointPos);
		jointTargetLine.SetPointPosition(1, targetPos);
	}
}
