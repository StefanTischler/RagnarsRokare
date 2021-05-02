﻿using HarmonyLib;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace RagnarsRokare.SlaveGreylings
{
    public partial class SlaveGreylings
    {
        [HarmonyPatch(typeof(BaseAI), "UpdateAI")]
        class BaseAI_UpdateAI_ReversePatch
        {
            [HarmonyReversePatch]
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void UpdateAI(BaseAI instance, float dt, ZNetView m_nview, ref float m_jumpInterval, ref float m_jumpTimer,
                ref float m_randomMoveUpdateTimer, ref float m_timeSinceHurt, ref bool m_alerted)
            {
                if (m_nview.IsOwner())
                {
                    instance.UpdateTakeoffLanding(dt);
                    if (m_jumpInterval > 0f)
                    {
                        m_jumpTimer += dt;
                    }
                    if (m_randomMoveUpdateTimer > 0f)
                    {
                        m_randomMoveUpdateTimer -= dt;
                    }
                    typeof(BaseAI).GetMethod("UpdateRegeneration", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(instance, new object[] { dt });
                    m_timeSinceHurt += dt;
                }
                else
                {
                    m_alerted = m_nview.GetZDO().GetBool("alert");
                }
            }
        }

        [HarmonyPatch(typeof(MonsterAI), "MakeTame")]
        static class MonsterAI_MakeTame_Patch
        {
            static void Postfix(MonsterAI __instance)
            {
                if (MobManager.IsControllableMob(__instance.name))
                {
                    var mobInfo = MobManager.GetMobInfo(__instance.name);
                    __instance.m_consumeItems.Clear();
                    __instance.m_consumeItems.AddRange(mobInfo.PostTameConsumables);
                    __instance.m_consumeSearchRange = 50;
                }
            }
        }

        [HarmonyPatch(typeof(MonsterAI), "UpdateAI")]
        static class MonsterAI_UpdateAI_Patch
        {
            static bool Prefix(MonsterAI __instance, float dt, ref ZNetView ___m_nview, ref Character ___m_character, ref float ___m_timeSinceHurt, 
                ref float ___m_jumpInterval, ref float ___m_jumpTimer, ref float ___m_randomMoveUpdateTimer, ref bool ___m_alerted)
            {
                if (!___m_character.IsTamed())
                {
                    return true;
                }
                if (!MobManager.IsControllableMob(__instance.name))
                {
                    return true;
                }
                string mobId = InitInstanceIfNeeded(__instance);
                if (string.IsNullOrEmpty(mobId)) return true;
                if (!___m_nview.IsOwner())
                {
                    return false;
                }
                if (__instance.IsSleeping())
                {
                    Invoke(__instance, "UpdateSleep", new object[] { dt });
                    Dbgl($"{___m_character.GetHoverName()}: Sleep updated");
                    return false;
                }

                BaseAI_UpdateAI_ReversePatch.UpdateAI(__instance, dt, ___m_nview, ref ___m_jumpInterval, ref ___m_jumpTimer, ref ___m_randomMoveUpdateTimer, ref ___m_timeSinceHurt, ref ___m_alerted);
                MobManager.Mobs[mobId].UpdateAI(dt);

                return false;
            }

            private static object Invoke(MonsterAI instance, string methodName, object[] argumentList)
            {
                return typeof(MonsterAI).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(instance, argumentList);
            }

            private static string InitInstanceIfNeeded(MonsterAI instance)
            {
                if (MobManager.IsControlledMob(instance.gameObject.GetInstanceID())) return MobManager.Instances[instance.gameObject.GetInstanceID()];

                var nview = typeof(BaseAI).GetField("m_nview", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(instance) as ZNetView;
                var uniqueId = nview.GetZDO().GetString(Constants.Z_CharacterId);

                var mob = MobManager.CreateMob(Common.GetPrefabName(instance.gameObject.name), instance);
                if (mob == null)
                {
                    Dbgl($"Failed to create mob of {Common.GetPrefabName(instance.gameObject.name)}");
                    return null;
                }

                if (MobManager.IsControlledMob(uniqueId))
                {
                    Dbgl($"Replacing old instance of mob '{nview.GetZDO().GetString(Constants.Z_GivenName)}'");
                    MobManager.Mobs[uniqueId] = mob;
                    MobManager.RemoveStaleInstance(uniqueId);
                    MobManager.Instances.Add(instance.gameObject.GetInstanceID(), uniqueId);
                }
                else
                {
                    Dbgl($"Adding new instance of mob '{nview.GetZDO().GetString(Constants.Z_GivenName)}'");
                    MobManager.Mobs.Add(uniqueId, mob);
                    MobManager.Instances.Add(instance.gameObject.GetInstanceID(), uniqueId);
                }
                return uniqueId;
            }
        }
    }
}
