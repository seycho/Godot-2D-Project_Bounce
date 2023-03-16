using Godot;
using System;

public partial class player_main : CharacterBody2D
{
	public int speedStop = 100;
	public int speedMax = 400;
	public int speedNormal = 2000;
	public int speedBrake = 1000;
	private Vector2 _velocity = new Vector2();
	private Vector2 _playerControl = new Vector2();
	private float secCount = 0;
	
	private float deltaTime;

	private void InputMove()
	{
		_playerControl = Vector2.Zero;
		if (Input.IsActionPressed("player_U"))
			_playerControl.Y -= 1;
		if (Input.IsActionPressed("player_D"))
			_playerControl.Y += 1;
		if (Input.IsActionPressed("player_L"))
			_playerControl.X -= 1;
		if (Input.IsActionPressed("player_R"))
			_playerControl.X += 1;
		_playerControl = _playerControl.Normalized();
		Vector2 _velNormal = _velocity.Normalized();
		bool _isNotControl = _playerControl == Vector2.Zero;

		_velocity += _playerControl * speedNormal * deltaTime;
		_velocity -= _velNormal * (float)Math.Pow(_velocity.Length()*0.1, 2) * deltaTime;
		if ((_isNotControl) & (_velocity.Length() < speedStop))
			_velocity *= 0;
	}

	private void InputDash()
	{
		if (Input.IsActionJustPressed("player_dash"))
		{
			_velocity += 3000 * _velocity.Normalized();
		}
	}
	
	private void InputPush()
	{
		if (Input.IsActionJustPressed("player_shieldpush"))
		{
			_velocity += 3000 * _velocity.Normalized();
		}
		
	}

	private void RotateShield()
	{
		float angleNode = GetNode<Marker2D>("posshield").Rotation;
		Vector2 directionNode = new Vector2((float)Math.Cos(angleNode), (float)Math.Sin(angleNode));
		Vector2 directionMouse = (GetGlobalMousePosition() - Position).Normalized();
		float outerProduction = directionNode.X * directionMouse.Y - directionNode.Y * directionMouse.X;
		float directionAngular = outerProduction / Math.Abs(outerProduction);
		float thetaDelta = (float)Math.Acos(directionNode.Dot(directionMouse));

		float theta0 = 3.141592f / 2;
		float angularVelocity0 = 3.141592f * 4;
		float angularVelocity = thetaDelta / theta0 * angularVelocity0;

		float angleDelta = directionAngular * angularVelocity * deltaTime;
		if (float.IsNaN(angleDelta))
			angleDelta = 0;
		GetNode<Marker2D>("posshield").Rotation = angleNode + angleDelta;
		GetNode<CharacterBody2D>("posshield/body").Rotation = angleDelta * 2;
	}

	public override void _Ready()
	{
		//Engine.SetTargetFps(60);
	}

	public override void _PhysicsProcess(double delta)
	{
		deltaTime = (float)delta;
		InputMove();
		InputDash();
		foreach (var body in GetNode<Area2D>("posshield/body/areahit").GetOverlappingBodies())
		{
			if (body.Name == "bullet")
			{
				_velocity += (Position - body.Position).Normalized() * ((RigidBody2D)body).LinearVelocity.Length() / 10;
				
			}
				
		}
		

		MoveAndCollide(_velocity * deltaTime);
		RotateShield();
	}
}
