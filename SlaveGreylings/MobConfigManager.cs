﻿using RagnarsRokare.MobAI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RagnarsRokare.SlaveGreylings
{
    public class MobConfig
    {
        public IEnumerable<ItemDrop> PreTameConsumables { get; set; }
        public IEnumerable<ItemDrop> PostTameConsumables { get; set; }
        public float PreTameFeedDuration { get; set; }
        public float PostTameFeedDuration { get; set; }
        public float TamingTime { get; set; }
        public string AIType { get; set; }
        public MobAIBaseConfig AIConfig { get; set; }
    }

    public static class MobConfigManager
    {
        public static bool IsControllableMob(string mobType)
        {
            var type = Common.GetPrefabName(mobType);
            if (type == "Greyling") return true;
            if (type == "Greydwarf_Elite") return true;
            return false;
        }

        public static MobConfig GetMobConfig(string mobType)
        {
            var type = Common.GetPrefabName(mobType);
            switch (type)
            {
                case "Greyling":
                    {

                        return new MobConfig
                        {
                            PostTameConsumables = GreylingsConfig.PostTameConsumables.Select(i => ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Material, i).FirstOrDefault()),
                            PostTameFeedDuration = GreylingsConfig.FeedDuration.Value,
                            PreTameConsumables = GreylingsConfig.PreTameConsumables.Select(i => ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Material, i).FirstOrDefault()),
                            PreTameFeedDuration = GreylingsConfig.FeedDuration.Value,
                            TamingTime = GreylingsConfig.TamingTime.Value,
                            AIType = "Worker",
                            AIConfig = new WorkerAIConfig
                            {
                                FeedDuration = GreylingsConfig.FeedDuration.Value,
                                IncludedContainers = GreylingsConfig.IncludedContainersList.Value.Split(','),
                                TimeBeforeAssignmentCanBeRepeated = GreylingsConfig.TimeBeforeAssignmentCanBeRepeated.Value,
                                TimeLimitOnAssignment = GreylingsConfig.TimeLimitOnAssignment.Value,
                                Agressiveness = GreylingsConfig.Agressiveness.Value,
                                Awareness = GreylingsConfig.Awareness.Value,
                                Mobility = GreylingsConfig.Mobility.Value,
                                Intelligence = GreylingsConfig.Intelligence.Value
                            }
                        };
                    }
                case "Greydwarf_Elite":
                    {
                        return new MobConfig
                        {
                            PostTameConsumables = BruteConfig.PostTameConsumables.Select(i => ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Material, i).FirstOrDefault()),
                            PostTameFeedDuration = BruteConfig.PostTameFeedDuration.Value,
                            PreTameConsumables = BruteConfig.PreTameConsumables.Select(i => ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Material, i).FirstOrDefault()),
                            PreTameFeedDuration = BruteConfig.PreTameFeedDuration.Value,
                            TamingTime = BruteConfig.TamingTime.Value,
                            AIType = "Fixer",
                            AIConfig = new FixerAIConfig
                            {
                                PostTameFeedDuration = BruteConfig.PostTameFeedDuration.Value,
                                IncludedContainers = BruteConfig.IncludedContainersList.Value.Split(','),
                                TimeLimitOnAssignment = BruteConfig.TimeLimitOnAssignment.Value,
                                Agressiveness = BruteConfig.Agressiveness.Value,
                                Awareness = BruteConfig.Awareness.Value,
                                Mobility = BruteConfig.Mobility.Value,
                                Intelligence = BruteConfig.Intelligence.Value
                            }
                        };
                    }
                default:
                    return null;
            }
        }
    }
}
