using Godot;
using System;

public class IKTest : Godot.Node2D
{
	#region Settings
	[Export]
	private int flipped = 1;
	//limb one = calf or forearm, limb two = thigh or upper arm
	[Export]
	private int limbOneLen = 100;
	[Export]
	private int limbTwoLen = 120;
	private int maxLen;
	[Export]
	private int minLen = 20;
	#endregion

	#region Working Variables
	private Vector2 targetPos = new Vector2();
	private Vector2 anchorPos = new Vector2();
	private Vector2 jointPos = new Vector2();
	private Vector2 endPos = new Vector2();
	private float anchorToEndAngle = 0;
	private float anchorEndDist = 0;
	private float anchorAngle = 0;
	#endregion

	#region Visual Indicators
	private Position2D anchorPosInd;
	private Position2D endPosInd;
	private Position2D jointPosInd;
	private Line2D anchorEndLine;
	private Line2D anchorJointLine;
	private Line2D jointEndLine;
	#endregion

	public override void _Ready()
	{
		maxLen = limbOneLen + limbTwoLen;
		anchorPosInd = (Position2D) GetNode("AnchorPos");
		endPosInd = (Position2D) GetNode("EndPos");
		jointPosInd = (Position2D) GetNode("JointPos");
		anchorEndLine = (Line2D) GetNode("AnchorEndLine");
		anchorJointLine = (Line2D) GetNode("AnchorJointLine");
		jointEndLine = (Line2D) GetNode("JointEndLine");
	}

	public override void _PhysicsProcess(float delta) 
	{
		CalculateIK();
		UpdateIKVisuals();
	}

	private void CalculateIK()
	{
		targetPos = GetGlobalMousePosition();
		// find the clamped end position from the anchor using the ideal end positon (mouse position)
		anchorToEndAngle = anchorPos.AngleToPoint(targetPos);
		anchorEndDist = Mathf.Clamp(anchorPos.DistanceTo(targetPos), minLen, maxLen);
		float endPosX = anchorEndDist * -Mathf.Cos(anchorToEndAngle);
		float endPosY = anchorEndDist * -Mathf.Sin(anchorToEndAngle);
		endPos = new Vector2(endPosX, endPosY);

		// law of cosines to find anchor angle
		float aSqr = limbOneLen * limbOneLen;
		float bSqr = limbTwoLen * limbTwoLen;
		float cSqr = anchorEndDist * anchorEndDist;
		float clampedLawOfCosines = Mathf.Min(1, Mathf.Max(-1, (bSqr + cSqr - aSqr)/(2 * limbTwoLen * anchorEndDist)));
		anchorAngle = flipped * Mathf.Acos(clampedLawOfCosines);

		// joint position
		var jointPosX = (limbTwoLen * Mathf.Cos(anchorAngle - anchorToEndAngle));
		var jointPosY = (limbTwoLen * Mathf.Sin(anchorAngle - anchorToEndAngle));
		jointPos = new Vector2(-jointPosX, jointPosY);
	}

	private void UpdateIKVisuals()
	{
		// update position indicators
		endPosInd.Position = endPos;
		jointPosInd.Position = jointPos;

		// update lines
		anchorEndLine.SetPointPosition(0, anchorPos);
		anchorEndLine.SetPointPosition(1, endPos);
		anchorJointLine.SetPointPosition(0, anchorPos);
		anchorJointLine.SetPointPosition(1, jointPos);
		jointEndLine.SetPointPosition(0, jointPos);
		jointEndLine.SetPointPosition(1, endPos);
	}
}
