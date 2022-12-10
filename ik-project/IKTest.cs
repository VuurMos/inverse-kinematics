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
	private float movingDirection = 1; //this should be velocity.angle() (as radians)
	private float motionCounter = 0.0f;
	#endregion

	#region Position Variables
	private Vector2 targetPosition = new Vector2();
	private Vector2 originPosition = new Vector2();
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
		/* // motion counter simulation
		motionCounter += 5.2f * Mathf.Pow(vel.Length() * 3, 0.4f);

		if (motionCounter > 360)
		{
			motionCounter = 0;
		}
		
		float animCounter = Mathf.Deg2Rad(motionCounter);

		// this updates based on movement speed
		float gait = (Mathf.Pow(vel.Length() * 2, 0.4f));

		/* float movingDirection = vel.Normalized().Angle();
		//GD.Print(movingDirection);
		float directionScale = Mathf.Cos(movingDirection);
		GD.Print(directionScale); */

		// this modifies the size of the gait based on leg length
		// however it is way too large currently when using these formula
		//float horizontalLenFactor = (maxLen / 4);
		//float verticalLenFactor = (maxLen / 6);
		//float horizontalLenFactor = 2;
		//float verticalLenFactor = 2;

		// this checks the movement angle to apply a modifier to the horizontal aspect
		// of the movement animation
		//float movingDirection = Mathf.Rad2Deg(vel.Angle());
		// find the offset
		//float ax = (gait * horizontalLenFactor * Mathf.Sin(motionCounter)) * Mathf.Cos(movingDirection);
		//float ay = gait * verticalLenFactor * (-Mathf.Cos(motionCounter) - 1); */


		//float pace = vel.Length() / 120;
		float pace = Mathf.Pow(vel.Length() * 0.1f, 0.4f);

		//The rate at which the motionCounter increases is the animation speed
		float animSpeed = 0.5f;
		motionCounter += animSpeed * pace; 

		if (motionCounter > 360)
		{
			motionCounter = 0;
		}

		float animCounter = Mathf.Deg2Rad(motionCounter);

		// (-Cos(animCounter) - 1) shifts the starting position of the movement
		// so that at it's maximum, the offset reaches the target position
		Vector2 baseOffset = new Vector2(
			Mathf.Sin(animCounter), 
			(-Mathf.Cos(animCounter) - 1)
		);
		
		// this determines the size of x and y offset in relation to the limb length
		Vector2 stepSize = new Vector2(
			maxLen / 12, 
			maxLen / 16
		);

		float ax = stepSize.x * pace * baseOffset.x;
		float ay = stepSize.y * pace * baseOffset.y;
		return new Vector2(ax, ay);
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
