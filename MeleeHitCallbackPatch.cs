using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace DismembermentSue
{
	[HarmonyPatch(typeof(Mission)), HarmonyPatch("MeleeHitCallback")]
	internal static class MeleeHitCallbackPatch
	{
		private static void Prefix(ref AttackCollisionData collisionData, Agent attacker, Agent victim, GameEntity realHitEntity, float momentumRemainingToComputeDamage, ref float inOutMomentumRemaining, ref MeleeCollisionReaction colReaction, CrushThroughState cts, Vec3 blowDir, Vec3 swingDir, ref object hprd, bool crushedThroughWithoutAgentCollision)
		{
			
			bool flag = collisionData.VictimHitBodyPart == BoneBodyPartType.Head && collisionData.StrikeType == 0 && collisionData.DamageType == 0 && (attacker.AttackDirection == Agent.UsageDirection.AttackLeft || attacker.AttackDirection == Agent.UsageDirection.AttackRight);
			if (flag)
			{
				//DismembermentSubModule.AddPDV(victim, attacker);
				DismembermentSettings.Instance().GetDismembermentMissionBusiness().AddPDV(victim, attacker);
			}
		}
	}
}
