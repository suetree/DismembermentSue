using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace DismembermentSue
{
    public  class DismembermentMissionBusiness 
    {

		public struct PotentialDismembermentVictim
		{
			public Agent agent;

			public Agent attacker;

			public float timer;
		}

		private  Random _random = new Random();

		private  int _randChance = 2;

		private  bool _slowMotion = true;

		private  bool _debug = false;

		private float slowMotionTimer;

		private  List<Agent> _ignoreAgents = new List<Agent>();

		public  List<PotentialDismembermentVictim> _pdv = new List<PotentialDismembermentVictim>();


		public DismembermentMissionBusiness()
		{
			string fullPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..\\..\\"));
			string text = File.ReadAllText(fullPath + "/Settings.cfg");
		    Dictionary<string, string> settingsDic = new Dictionary<string, string>();
		    string[] array = text.Split(new char[]
			{
				'\n'
			});
			for (int i = 0; i < array.Length; i++)
			{
				bool flag = array[i].StartsWith("//");
				if (!flag)
				{
					settingsDic.Add(array[i].Split(new char[]
					{
						'='
					})[0], array[i].Split(new char[]
					{
						'='
					})[1]);
				}
			}
			_randChance = int.Parse(settingsDic["Chance"]);
			_slowMotion = bool.Parse(settingsDic["SlowMotion"]);
			_debug = bool.Parse(settingsDic["Debug"]);
		}

		public  void AddPDV(Agent agent, Agent attacker)
		{
			bool flag = Mission.Current.Scene.GetName().ToLower().Contains("arena");
			if (flag)
			{
				DisplayDebugMessage("Tried to add PDV in arena");
			}
			else
			{
				bool flag2 = agent == Agent.Main;
				if (flag2)
				{
					DisplayDebugMessage("Tried to add the player as a PDV...");
				}
				else
				{
					bool flag3 = !agent.IsHuman;
					if (flag3)
					{
						DisplayDebugMessage("Tried to add a non-human agent to PDV");
					}
					else
					{
						foreach (PotentialDismembermentVictim current in _pdv)
						{
							bool flag4 = current.agent == agent;
							if (flag4)
							{
								DisplayDebugMessage("Tried to add " + agent.Name + " to PDV but already in it!");
								return;
							}
						}
						foreach (Agent current2 in _ignoreAgents)
						{
							bool flag5 = current2 == agent;
							if (flag5)
							{
								DisplayDebugMessage("Tried to add " + agent.Name + " to PDV but it is supposed to be ignored (clone / dismembered)");
								return;
							}
						}
						bool flag6 = _random.Next(1, _randChance + 1) != 1;
						//bool flag6 = false;
						if (flag6)
						{
							DisplayDebugMessage("Didn't add PDV because of random chance");
						}
						else
						{
							_pdv.Add(new PotentialDismembermentVictim
							{
								agent = agent,
								attacker = attacker,
								timer = MBCommon.TimeType.Mission.GetTime() + 0.0001f
							});
							DisplayDebugMessage("Added " + agent.Name + " to PDV");
						}
					}
				}
			}
		}

		public  void OnApplicationTick(float dt)
		{
			bool flag = Game.Current == null;
			if (!flag)
			{
				bool flag2 = Game.Current.CurrentState > Game.State.Running;
				if (!flag2)
				{
					bool flag3 = Mission.Current == null || Mission.Current.Scene == null;
					if (flag3)
					{
						if(null == _ignoreAgents)
						{
							DisplayDebugMessage("ignoreAgents = null");
						}
						bool flag4 = _ignoreAgents.Count > 0;
						if (flag4)
						{
							_ignoreAgents.Clear();
							DisplayDebugMessage("Cleared ignore agents");
						}
					}
					else
					{
						Agent main = Agent.Main;
						bool flag5 = main == null;
						if (!flag5)
						{
							PotentialDismembermentVictim[] array = _pdv.ToArray();
							for (int i = 0; i < array.Length; i++)
							{
								PotentialDismembermentVictim potentialDismembermentVictim = array[i];
								bool flag6 = !potentialDismembermentVictim.agent.IsActive() && potentialDismembermentVictim.agent.State == AgentState.Killed;
								if (flag6)
								{
									DisplayDebugMessage(potentialDismembermentVictim.agent.Name + " was dismembered!");
									bool flag7 = potentialDismembermentVictim.attacker == main;
									if (flag7)
									{
										DisplayMessage("DISMEMBERED!", 16711680u);
										bool flag8 = _slowMotion;
										if (flag8)
										{
											this.slowMotionTimer = MBCommon.TimeType.Mission.GetTime() + 0.5f;
											Mission.Current.Scene.SlowMotionMode = true;
											DisplayDebugMessage("Slow motion on");
										}
									}
									potentialDismembermentVictim.attacker.SetWantsToYell();
									this.DismemberHead(potentialDismembermentVictim.agent);
									_pdv.Remove(potentialDismembermentVictim);
								}
								bool flag9 = MBCommon.TimeType.Mission.GetTime() >= potentialDismembermentVictim.timer;
								if (flag9)
								{
									bool flag10 = potentialDismembermentVictim.agent.State != AgentState.Killed;
									if (flag10)
									{
										DisplayDebugMessage(potentialDismembermentVictim.agent.Name + " was not actually a kill, removing from PDV.");
										_pdv.Remove(potentialDismembermentVictim);
										return;
									}
									bool flag11 = potentialDismembermentVictim.agent.State == AgentState.Unconscious;
									if (flag11)
									{
										DisplayDebugMessage(potentialDismembermentVictim.agent.Name + " is unconscious, removing from PDV.");
										_pdv.Remove(potentialDismembermentVictim);
										return;
									}
								}
							}
							bool flag12 = _slowMotion && this.slowMotionTimer > 0f;
							if (flag12)
							{
								bool flag13 = MBCommon.TimeType.Mission.GetTime() >= this.slowMotionTimer;
								if (flag13)
								{
									this.slowMotionTimer = 0f;
									Mission.Current.Scene.SlowMotionMode = false;
									DisplayDebugMessage("Slow motion off");
								}
							}
							bool flag14 = Input.IsKeyPressed(InputKey.O) && _debug;
							if (flag14)
							{
								DisplayDebugMessage(Mission.Current.Scene.GetName());
							}
						}
					}
				}
			}
		}

		private void KillAgent(Agent agent)
		{
			bool flag = !GameNetwork.IsClientOrReplay;
			if (flag)
			{
				Blow blow = new Blow(agent.Index);
				blow.DamageType = DamageTypes.Cut;
				blow.StrikeType = StrikeType.Swing;
				blow.BoneIndex = agent.Monster.HeadLookDirectionBoneIndex;
				blow.Position = agent.Position;
				blow.Position.z = blow.Position.z + agent.GetEyeGlobalHeight();
				blow.BaseMagnitude = 2000f;
				blow.WeaponRecord.FillWith(null, -1, -1);
				blow.InflictedDamage = 2000;
				Vec3 v = new Vec3(1f, 0f, 0f, -1f);
				bool flag2 = Mission.Current.InputManager.IsGameKeyDown(2);
				if (flag2)
				{
					v = new Vec3(-1f, 0f, 0f, -1f);
				}
				else
				{
					bool flag3 = Mission.Current.InputManager.IsGameKeyDown(3);
					if (flag3)
					{
						v = new Vec3(1f, 0f, 0f, -1f);
					}
					else
					{
						bool flag4 = Mission.Current.InputManager.IsGameKeyDown(1);
						if (flag4)
						{
							v = new Vec3(0f, -1f, 0f, -1f);
						}
						else
						{
							bool flag5 = Mission.Current.InputManager.IsGameKeyDown(0);
							if (flag5)
							{
								v = new Vec3(0f, 1f, 0f, -1f);
							}
						}
					}
				}
				blow.SwingDirection = agent.Frame.rotation.TransformToParent(v);
				blow.SwingDirection.Normalize();
				blow.Direction = blow.SwingDirection;
				blow.DamageCalculated = true;
				agent.RegisterBlow(blow);
			}
		}

		private void DismemberHead(Agent agent)
		{
			bool flag = agent.IsActive();
			if (flag)
			{
				this.KillAgent(agent);
			}
			_ignoreAgents.Add(agent);
			agent.AgentVisuals.SetVoiceDefinitionIndex(-1, 0f);
			AgentBuildData agentBuildData = new AgentBuildData(agent.Character);
			agentBuildData.NoHorses(true);
			agentBuildData.NoWeapons(true);
			agentBuildData.NoArmor(false);
			agentBuildData.Team(Mission.Current.PlayerEnemyTeam);
			agentBuildData.TroopOrigin(agent.Origin);
			agentBuildData.InitialFrame(new MatrixFrame
			{
				origin = agent.Position + agent.LookDirection * -0.75f,
				rotation = agent.Frame.rotation
			});
			Agent agent2 = Mission.Current.SpawnAgent(agentBuildData, false, 0);
			_ignoreAgents.Add(agent2);
			IEnumerable<object> meshes = agent2.AgentVisuals.GetSkeleton().GetAllMeshes();
			foreach (Mesh current in meshes)
			{
				bool flag2 = !current.Name.ToLower().Contains("head") && !current.Name.ToLower().Contains("hair") && !current.Name.ToLower().Contains("beard") && !current.Name.ToLower().Contains("eyebrow") && !current.Name.ToLower().Contains("helmet") && !current.Name.ToLower().Contains("_cap_");
				if (flag2)
				{
					current.SetVisibilityMask(VisibilityMaskFlags.EditModeAny);
					//current.ClearMesh();
				}
			}
			agent2.AgentVisuals.GetEntity().ActivateRagdoll();
			agent2.AgentVisuals.SetVoiceDefinitionIndex(-1, 0f);
			this.KillAgent(agent2);
			foreach (Mesh current2 in agent.AgentVisuals.GetSkeleton().GetAllMeshes())
			{
				bool flag3 = current2.Name.ToLower().Contains("head") || current2.Name.ToLower().Contains("hair") || current2.Name.ToLower().Contains("beard") || current2.Name.ToLower().Contains("eyebrow") || current2.Name.ToLower().Contains("helmet") || current2.Name.ToLower().Contains("_cap_");
				if (flag3)
				{
					current2.SetVisibilityMask(VisibilityMaskFlags.EditModeAny);
					//current2.ClearMesh();
				}
			}
			MatrixFrame boneEntitialFrameWithIndex = agent.AgentVisuals.GetSkeleton().GetBoneEntitialFrameWithIndex((byte)agent.BoneMappingArray[HumanBone.Head]);
			Vec3 vec = agent.AgentVisuals.GetGlobalFrame().TransformToParent(boneEntitialFrameWithIndex.origin);
			agent.CreateBloodBurstAtLimb(13, ref vec, 0.5f + MBRandom.RandomFloat * 0.5f);
			boneEntitialFrameWithIndex = agent2.AgentVisuals.GetSkeleton().GetBoneEntitialFrameWithIndex((byte)agent2.BoneMappingArray[HumanBone.Head]);
			vec = agent2.AgentVisuals.GetGlobalFrame().TransformToParent(boneEntitialFrameWithIndex.origin);
			agent2.CreateBloodBurstAtLimb(13, ref vec, 0.5f + MBRandom.RandomFloat * 0.5f);
		}


		private void DisplayDebugMessage(string message)
		{
			if (_debug)  InformationManager.DisplayMessage(new InformationMessage(message));
		}

		private  void DisplayMessage(string msg, uint color)
		{
			if (_debug)
			{
				InformationManager.DisplayMessage(new InformationMessage(msg, Color.FromUint(color)));
			}
			
		}
	}

}
