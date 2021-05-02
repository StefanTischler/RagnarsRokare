﻿using RagnarsRokare.MobAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace RagnarsRokare.SlaveGreylings
{
    public static class MobManager
    {
        private static readonly Dictionary<string, MobInfo> m_mobControllers = new Dictionary<string, MobInfo>();

        static MobManager()
        {
            foreach (var mobController in GetAllControllableMobTypes())
            {
                try
                {
                    var instance = Activator.CreateInstance(mobController) as IControllableMob;
                    var mobInfo = instance.GetMobInfo();
                    m_mobControllers.Add(mobInfo.Name, mobInfo);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to instanciate type:{e.Message}");
                }
            }
        }
        public static Dictionary<string, MobAIBase> Mobs = new Dictionary<string, MobAIBase>();
        public static Dictionary<int, string> Instances = new Dictionary<int, string>();


        public static bool IsControlledMob(string id)
        {
            return string.IsNullOrEmpty(id) ? false : Mobs.ContainsKey(id);
        }

        public static bool IsControlledMob(int instanceId)
        {
            return Instances.ContainsKey(instanceId);
        }

        public static void RemoveStaleInstance(string mobUniqueId)
        {
            if (Instances.ContainsValue(mobUniqueId))
            {
                Instances.Remove(Instances.Single(i => i.Value == mobUniqueId).Key);
            }
        }

        public static bool IsControllableMob(string mobName)
        {
            var name = Common.GetPrefabName(mobName);
            return m_mobControllers.ContainsKey(name);
        }

        public static MobInfo GetMobInfo(string mobName)
        {
            var name = Common.GetPrefabName(mobName);
            return m_mobControllers.ContainsKey(name) ? m_mobControllers[name] : null;
        }

        public static MobAIBase CreateMob(string mobName, BaseAI baseAI)
        {
            if (!m_mobControllers.ContainsKey(mobName)) return null;

            var mobType = m_mobControllers[mobName].AIType;
            return Activator.CreateInstance(mobType, new object[]{ baseAI }) as MobAIBase;
        }

        private static IEnumerable<Type> GetAllControllableMobTypes()
        {
            var it = typeof(IControllableMob);
            var asm = Assembly.GetExecutingAssembly();
            return asm.GetLoadableTypes().Where(it.IsAssignableFrom).Where(t => !(t.Equals(it))).ToList();
        }
    }
}
