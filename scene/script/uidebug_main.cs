using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class uidebug_main : CanvasLayer
{
	public int IsActive = 1;
	public string pathPlayer = "player";
	public string pathBoss = "boss";
	public string pathBulletSpawner = "boss/spawbulletrand";
	public string pathBulletPack = "bulletpak";

	private CharacterBody2D nodePlayer;
	private player_main scriptPlayer;
	private RigidBody2D nodeBoss;
	private bossnor1_main scriptBoss;
	private Node2D nodeBulletSpawner;
	private spawbulletrand_main scriptBulletSpawner;
	private Node2D nodeBulletPack;
	private double timeProcess = 0;
	private double timeUpdate = 0;

	private void ShowPhysics()
	{
		Vector2 _position;
		string _x;
		string _y;
		Vector2 _velocity;
		string _v;
		string _d;

		_position = nodePlayer.Position;
		_x = _position.X.ToString("F1");
		_y = _position.Y.ToString("F1");
		GetNode<Label>("panelR/phy/player/x").Text = _x;
		GetNode<Label>("panelR/phy/player/y").Text = _y;
		_velocity = nodePlayer.Velocity;
		_v = _velocity.Length().ToString("F1");
		_d = (_velocity.Angle() / Math.PI * 360).ToString("F1");
		GetNode<Label>("panelR/phy/player/v").Text = _v;
		GetNode<Label>("panelR/phy/player/d").Text = _d;

		_position = nodeBoss.Position;
		_x = _position.X.ToString("F1");
		_y = _position.Y.ToString("F1");
		GetNode<Label>("panelR/phy/boss/x").Text = _x;
		GetNode<Label>("panelR/phy/boss/y").Text = _y;
		_velocity = scriptBoss.Velocity;
		_v = _velocity.Length().ToString("F1");
		_d = (_velocity.Angle() / Math.PI * 360).ToString("F1");
		GetNode<Label>("panelR/phy/boss/v").Text = _v;
		GetNode<Label>("panelR/phy/boss/d").Text = _d;
	}

	private void ShowStatus()
	{
		float _hp;
		float _mp;

		_hp = scriptPlayer.HealPoint;
		_mp = scriptPlayer.ManaPoint;
		GetNode<Label>("panelR/status/player/hp").Text = _hp.ToString("F2");
		GetNode<Label>("panelR/status/player/mp").Text = _mp.ToString("F2");
		_hp = scriptBoss.HealPoint;
		_mp = scriptBoss.ManaPoint;
		GetNode<Label>("panelR/status/boss/hp").Text = _hp.ToString("F2");
		GetNode<Label>("panelR/status/boss/mp").Text = _mp.ToString("F2");
	}

	private void ShowBulletNum()
	{
		int _numP = 0;
		int _numE = 0;
		foreach (var _node in nodeBulletPack.GetChildren())
		{
			if (_node.IsInGroup("player"))
				_numP++;
			if (_node.IsInGroup("enemy"))
				_numE++;
		}
		GetNode<Label>("panelR/bullet/num/player").Text = _numP.ToString();
		GetNode<Label>("panelR/bullet/num/enemy").Text = _numE.ToString();
	}

	private void ShowRemainTime()
	{
		GetNode<Label>("panelR/remain/hitplayer/duration").Text = scriptPlayer.Duration["player_hit"][0].ToString("F2");
		GetNode<Label>("panelR/remain/hitplayer/trigger").Text = scriptPlayer.Trigger["player_hit"][0].ToString("F2");
		GetNode<Label>("panelR/remain/dashplayer/cooldown").Text = scriptPlayer.Cooldown["player_dash"][0].ToString("F2");
		GetNode<Label>("panelR/remain/dashplayer/trigger").Text = scriptPlayer.Trigger["player_dash"][0].ToString("F2");
		GetNode<Label>("panelR/remain/pushshield/cooldown").Text = scriptPlayer.Cooldown["shield_push"][0].ToString("F2");
		GetNode<Label>("panelR/remain/pushshield/trigger").Text = scriptPlayer.Trigger["shield_push"][0].ToString("F2");

		GetNode<Label>("panelR/remain/hitboss/duration").Text = scriptBoss.Duration["enemy_hit"][0].ToString("F2");
		GetNode<Label>("panelR/remain/hitboss/trigger").Text = scriptBoss.Trigger["enemy_hit"][0].ToString("F2");
		GetNode<Label>("panelR/remain/aimrandom/cooldown").Text = scriptBoss.Cooldown["random_aim"][0].ToString("F2");

		GetNode<Label>("panelR/remain/spawnbullet/cooldown").Text = scriptBulletSpawner.Cooldown["spawn"][0].ToString("F2");
	}

	private void ShowElaspedTime()
	{
		GetNode<Label>("panelL/time/num").Text = timeProcess.ToString("N0");
	}

	private void ShowFPS()
	{
		GetNode<Label>("panelL/fps/num").Text = Engine.GetFramesPerSecond().ToString();
	}

	private void SetPanelModulateAtInput(string _nameKey, string _pathNode)
	{
		if (Input.IsActionJustPressed(_nameKey))
			GetNode<Panel>(_pathNode).Modulate = new Color(1, 1, 1, 1);
		if (Input.IsActionJustReleased(_nameKey))
			GetNode<Panel>(_pathNode).Modulate = new Color(1, 1, 1, 0);
	}

	private void ShowInputKey()
	{
		SetPanelModulateAtInput("player_U", "panelL/key/cover/0/1");
		SetPanelModulateAtInput("player_L", "panelL/key/cover/1/0");
		SetPanelModulateAtInput("player_D", "panelL/key/cover/1/1");
		SetPanelModulateAtInput("player_R", "panelL/key/cover/1/2");
		SetPanelModulateAtInput("player_dash", "panelL/key/cover/2/0");
		SetPanelModulateAtInput("player_shieldpush", "panelL/key/cover/2/1");
		SetPanelModulateAtInput("player_skilldraw", "panelL/key/cover/2/2");
	}

	private async Task UpdateInfosAtTime()
	{
		await Task.Run( ()=>
		{
			ShowPhysics();
			ShowStatus();
			ShowBulletNum();
			ShowRemainTime();

			ShowElaspedTime();
			ShowFPS();
			ShowInputKey();
		});
	}

	private async Task UpdateInfosEvery()
	{
		await Task.Run( ()=>
		{
			ShowInputKey();
		});
	}

	private void UpdateNodePath()
	{
		nodePlayer = GetOwner<Node2D>().GetNode<CharacterBody2D>(pathPlayer);
		scriptPlayer = nodePlayer.GetNode<player_main>(".");
		nodeBoss = GetOwner<Node2D>().GetNode<RigidBody2D>(pathBoss);
		scriptBoss = nodeBoss.GetNode<bossnor1_main>(".");
		nodeBulletSpawner = GetOwner<Node2D>().GetNode<Node2D>(pathBulletSpawner);
		scriptBulletSpawner = nodeBulletSpawner.GetNode<spawbulletrand_main>(".");
		nodeBulletPack = GetOwner<Node2D>().GetNode<Node2D>(pathBulletPack);
	}

	public override void _Ready()
	{
		UpdateNodePath();
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
				UpdateInfosAtTime();
			}
			else
				timeUpdate -= delta;
			UpdateInfosEvery();
		}
	}
}
