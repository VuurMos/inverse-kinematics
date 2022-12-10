using Godot;
using System;

public class IKTest : Godot.Node2D
{
	#region Settings
	[Export]
	private int flipped = 1;
	[Export]
	private bool animated = true;
	//limb A = calf or forearm, limb B = thigh or upper arm
	[Export]
	private float animSpeed = 1f;
	[Export]
	private int limbALen = 120;
	private float aSqr;
	[Export]
	private int limbBLen = 100;
	private float bSqr;
	private int maxLen;
	[Export]
	private int minLen = 20;
	#endregion

	#region Movement Tracking
	private float motionCounter = 0.0f;
	#endregion

	#region Position Variables
	private Vector2 targetPosition = new Vector2();
	private Vector2 jointPosition = new Vector2();
	private Vector2 endPosition = new Vector2();
	#endregion

	#region Visual Indicators
	private Position2D originPosInd;
	private Position2D endPosInd;
	private Position2D jointPosInd;
	private Line2D originEndLine;
	private Line2D originJointLine;
	private Line2D jointEndLine;
	#endregion

	public override void _Ready()
	{
		GetNodes();
		InitLenCalcs();
	}

	private void GetNodes()
	{
		originPosInd = (Position2D) GetNode("OriginPos");
		endPosInd = (Position2D) GetNode("EndPos");
		jointPosInd = (Position2D) GetNode("JointPos");
		originEndLine = (Line2D) GetNode("OriginEndLine");
		originJointLine = (Line2D) GetNode("OriginJointLine");
		jointEndLine = (Line2D) GetNode("JointEndLine");
	}

	private void InitLenCalcs()
	{
		maxLen = limbALen + limbBLen;
		aSqr = limbALen * limbALen;
		bSqr = limbBLen * limbBLen;
	}

	private Vector2 GetAnimtargetPosition(Vector2 vel)
	{	
		// velocity modifier which applies to the animation speed and step size
		// at a max vel of 120 = ~10.5
		// note: could see if swapping this for a 0-1 easing works better so that
		// step size can be easily set as the maximum value
		// note: motion counter + vel mod could be moved to an IK manager later
		// so that it can be reused for all animations
		float velMod = Mathf.Pow(vel.Length() * 3f, 0.4f);

		// motion counter increase = animation speed
		motionCounter += animSpeed * velMod; 

		if (motionCounter > 360)
		{
			motionCounter = 0;
		}

		float animCounter = Mathf.Deg2Rad(motionCounter);

		// this determines the size of x and y offset in relation to the limb length
		Vector2 stepSize = new Vector2(
			3f, 
			2f
		);

		// (-Cos(animCounter) - 1) shifts the position of the animation
		// so that at it's maximum, the offset reaches the target position
		Vector2 baseOffset = new Vector2(
			Mathf.Sin(animCounter), 
			(-Mathf.Cos(animCounter) - 1)
		);
		
		// used to scale the horizontal step movement based on direction being travelled
		float movingDirection = Mathf.Cos(vel.Normalized().Angle());

		//return the animated offset
		return new Vector2(
			stepSize.x * velMod * baseOffset.x * movingDirection, 
			stepSize.y * velMod * baseOffset.y
		);
	}

	private void UpdateIK(Vector2 targetPos, Vector2 velocity)
	{
		// note: the target position has to be in range for the animated offset to work correctly.
		if (animated)
		{
			var animPos = GetAnimtargetPosition(velocity);
			targetPosition = new Vector2(targetPos.x + animPos.x, targetPos.y + animPos.y);
		}
		else
		{
			targetPosition = targetPos;
		}

		// set key variables using target position
		// note: global position should refer to the global position of the IK limb origin point
		float originToEndAngle = GlobalPosition.AngleToPoint(targetPosition);
		float originEndDist = Mathf.Clamp(GlobalPosition.DistanceTo(targetPosition), minLen, maxLen);
		float cSqr = originEndDist * originEndDist;

		// find the end position as restricted by the total limb length
		endPosition = new Vector2(
			originEndDist * -Mathf.Cos(originToEndAngle), 
			originEndDist * -Mathf.Sin(originToEndAngle)
		);

		// law of cosines to find origin angle
		float lawOfCosinesCalc = (bSqr + cSqr - aSqr)/(2 * limbBLen * originEndDist);
		float originAngle = flipped * Mathf.Acos(Mathf.Min(1, Mathf.Max(-1, lawOfCosinesCalc)));

		// joint position
		jointPosition = new Vector2(
			-limbBLen * Mathf.Cos(originAngle - originToEndAngle), 
			limbBLen * Mathf.Sin(originAngle - originToEndAngle)
		);

		UpdateIKVisuals();
	}

	private void UpdateIKVisuals()
	{
		// note: Position refers to the local ik origin position
		// update position indicators
		originPosInd.Position = Position;
		endPosInd.Position = endPosition;
		jointPosInd.Position = jointPosition;

		// update lines
		originEndLine.SetPointPosition(0, Position);
		originEndLine.SetPointPosition(1, endPosition);
		originJointLine.SetPointPosition(0, Position);
		originJointLine.SetPointPosition(1, jointPosition);
		jointEndLine.SetPointPosition(0, jointPosition);
		jointEndLine.SetPointPosition(1, endPosition);
	}
}
