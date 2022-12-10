using Godot;
using System;

public class IKTest : Godot.Node2D
{
	#region Settings
	[Export]
	private bool is3D = true;
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
	private Position2D ikTarget;
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
		ikTarget = (Position2D) GetNode("IKTarget");
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
		// so that it can be reused for all animations such as bouncing of torso/head
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

	private Vector2 Get3DJointPos(Vector2 jointPos, float originAng, Vector2 endPos)
	{
		// note: these two values can be applied to other animations to provide 
		// look offsets
		float facingDirection = (GlobalPosition - GetGlobalMousePosition()).Angle();
		float jointMod = GlobalPosition.x - (GlobalPosition.x + 1 * Mathf.Cos(facingDirection));
		
		// find the intersect point of the origin-end line
		float intDist = limbBLen * Mathf.Cos(originAng);
		float intAng = (GlobalPosition + endPos).AngleToPoint(GlobalPosition);

		Vector2 originEndIntersect = new Vector2(
			intDist * Mathf.Cos(intAng),
			intDist * Mathf.Sin(intAng)
		);

		// find the 3D joint position
		float jointIntDist = originEndIntersect.DistanceTo(jointPos);
		float jointIntAng = originEndIntersect.AngleToPoint(jointPos);

		Vector2 jointPos3D = new Vector2(
			originEndIntersect.x + (jointIntDist * -jointMod * Mathf.Cos(jointIntAng)),
			originEndIntersect.y + (jointIntDist * -jointMod * Mathf.Sin(jointIntAng))
		);

		return jointPos3D;
	}

	private void UpdateIK(Vector2 velocity)
	{
		targetPosition = ikTarget.GlobalPosition;
		// note: the target position has to be in range for the animated offset to work correctly.
		if (animated)
		{
			var animPos = GetAnimtargetPosition(velocity);
			targetPosition = new Vector2(targetPosition.x + animPos.x, targetPosition.y + animPos.y);
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
		// can multiply the origin angle by 1/-1 to flip it for limbs which don't use
		// 3D effect
		float lawOfCosinesCalc = (bSqr + cSqr - aSqr)/(2 * limbBLen * originEndDist);
		float originAngle = Mathf.Acos(Mathf.Min(1, Mathf.Max(-1, lawOfCosinesCalc)));

		// joint position
		jointPosition = new Vector2(
			-limbBLen * Mathf.Cos(originAngle - originToEndAngle), 
			limbBLen * Mathf.Sin(originAngle - originToEndAngle)
		);

		if (is3D)
		{
			//jointPosition = Get3DJointPos(jointPosition, originAngle);
			jointPosition = Get3DJointPos(jointPosition, originAngle, endPosition);
		}

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
