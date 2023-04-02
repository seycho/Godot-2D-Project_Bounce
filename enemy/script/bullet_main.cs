using Godot;
using System;

public partial class bullet_main : RigidBody2D
{
	public float SpeedProcess = 1.0f;

	private bool isActive = true;
	private float velDefault = 1000;

	public void ReflectShield()
	{
		RemoveFromGroup("enemy");
		AddToGroup("player");
		CollisionMask -= 2;
		GetNode<Sprite2D>("ballyellow").Visible = false;
	}

	public void HitPlayer()
	{
		QueueFree();
	}

	public void HitEnemy()
	{
		QueueFree();
	}

	private void StopActive()
	{
		isActive = false;
		GetNode<Timer>("remove").Start();
	}
	
	private void ActionRemove()
	{
		if (GetNode<AudioStreamPlayer>("sound/crash").Playing == false)
			GetNode<AudioStreamPlayer>("sound/crash").Play();
		Visible = false;
		LinearVelocity = Vector2.Zero;
		CollisionLayer = 0;
		CollisionMask = 0;
		GetNode<Area2D>("regdet").CollisionLayer = 0;
		GetNode<Area2D>("regdet").CollisionMask = 0;
		GetNode<Area2D>("regatk").CollisionLayer = 0;
		GetNode<Area2D>("regatk").CollisionMask = 0;
		if (GetNode<Timer>("remove").TimeLeft <= 0)
		{
			GetNode<AudioStreamPlayer>("sound/crash").Stop();
			QueueFree();
		}
	}

	private void KeepSpeed()
	{
		bool isCollision = false;
		foreach (var _body in GetNode<Area2D>("regdet").GetOverlappingBodies())
		{
			if (_body.Name != Name)
				isCollision = true;
		}
		if (isCollision)
		{
			GetNode<AudioStreamPlayer>("sound/hit").Play();
			LinearVelocity = LinearVelocity / LinearVelocity.Length() * velDefault * SpeedProcess;
		}
	}

	private void ColliBullet()
	{
		foreach (var _body in GetNode<Area2D>("regatk").GetOverlappingBodies())
		{
			if (IsInGroup("player") & _body.IsInGroup("enemy"))
			{
				StopActive();
			}
			if (IsInGroup("enemy") & _body.IsInGroup("player"))
			{
				StopActive();
			}
		}
	}

	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
		if (isActive)
		{
			KeepSpeed();
			ColliBullet();
		}
		else
		{
			ActionRemove();
		}
	}
}
