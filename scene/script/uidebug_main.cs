using Godot;
using System;
using System.Threading.Tasks;

public partial class uidebug_main : CanvasLayer
{
	public int IsActive = 1;
	public string pathPlayer = "player";
	public string pathBoss = "boss";
	public string pathBulletPack = "boss/bulletpak";

	private double timeProcess = 0;
	private double timeUpdate = 0;

	private void ShowElaspedTime()
	{
		GetNode<Label>("panelL/time/num").Text = timeProcess.ToString("N0");
	}

	private void ShowPhysics()
	{
		Vector2 _position;
		string _x;
		string _y;
		Vector2 _velocity;
		string _v;
		string _d;

		_position = GetOwner<Node2D>().GetNode<CharacterBody2D>(pathPlayer).Position;
		_x = _position.X.ToString("F1");
		_y = _position.Y.ToString("F1");
		GetNode<Label>("panelR/phy/player/x").Text = _x;
		GetNode<Label>("panelR/phy/player/y").Text = _y;
		_velocity = GetOwner<Node2D>().GetNode<CharacterBody2D>(pathPlayer).Velocity;
		_v = _velocity.Length().ToString("F1");
		_d = (_velocity.Angle() / Math.PI * 360).ToString("F1");
		GetNode<Label>("panelR/phy/player/v").Text = _v;
		GetNode<Label>("panelR/phy/player/d").Text = _d;

		_position = GetOwner<Node2D>().GetNode<RigidBody2D>(pathBoss).Position;
		_x = _position.X.ToString("F1");
		_y = _position.Y.ToString("F1");
		GetNode<Label>("panelR/phy/boss/x").Text = _x;
		GetNode<Label>("panelR/phy/boss/y").Text = _y;
		_velocity = GetOwner<Node2D>().GetNode<RigidBody2D>(pathBoss).GetNode<bossnor1_main>(".").Velocity;
		_v = _velocity.Length().ToString("F1");
		_d = (_velocity.Angle() / Math.PI * 360).ToString("F1");
		GetNode<Label>("panelR/phy/boss/v").Text = _v;
		GetNode<Label>("panelR/phy/boss/d").Text = _d;
	}

	private void ShowStatus()
	{
		float _hp;
		float _mp;

		_hp = GetOwner<Node2D>().GetNode<CharacterBody2D>(pathPlayer).GetNode<player_main>(".").HealPoint;
		_mp = GetOwner<Node2D>().GetNode<CharacterBody2D>(pathPlayer).GetNode<player_main>(".").ManaPoint;
		GetNode<Label>("panelR/status/player/hp").Text = _hp.ToString("F2");
		GetNode<Label>("panelR/status/player/mp").Text = _mp.ToString("F2");
		_hp = GetOwner<Node2D>().GetNode<RigidBody2D>(pathBoss).GetNode<bossnor1_main>(".").HealPoint;
		_mp = GetOwner<Node2D>().GetNode<RigidBody2D>(pathBoss).GetNode<bossnor1_main>(".").ManaPoint;
		GetNode<Label>("panelR/status/boss/hp").Text = _hp.ToString("F2");
		GetNode<Label>("panelR/status/boss/mp").Text = _mp.ToString("F2");
	}

	private void ShowBulletNum()
	{
		int _numP = 0;
		int _numE = 0;
		foreach (var _node in GetOwner<Node2D>().GetNode<Node2D>("bulletpak").GetChildren())
		{
			if (_node.IsInGroup("player"))
				_numP++;
			if (_node.IsInGroup("enemy"))
				_numE++;
		}
		GetNode<Label>("panelR/bullet/num/player").Text = _numP.ToString();
		GetNode<Label>("panelR/bullet/num/enemy").Text = _numE.ToString();
	}

	private async Task UpdateInfos()
	{
		await Task.Run( ()=>
		{
			ShowElaspedTime();
			ShowPhysics();
			ShowStatus();
			ShowBulletNum();
			GetNode<Label>("panelL/fps/num").Text = Engine.GetFramesPerSecond().ToString();
		});
	}

	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
		timeProcess += delta;
		if (Input.IsActionJustPressed("show_debug"))
		{
			IsActive *= -1;
			Visible = false;
		}
		if (IsActive > 0)
		{
			Visible = true;
			if (timeUpdate < 0)
			{
				timeUpdate = 0.2;
				UpdateInfos();
			}
			else
				timeUpdate -= delta;
		}
	}
}
