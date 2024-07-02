﻿using Gear;

namespace ProgressionWeapons.Utils
{
    internal static class GearIDRangeExtensions
    {
        public static uint GetOfflineID(this GearIDRange gearIDRange)
        {
            string itemInstanceId = gearIDRange.PlayfabItemInstanceId;
            if (!itemInstanceId.Contains("OfflineGear_ID_"))
            {
                PWLogger.Error($"Find PlayfabItemInstanceId without substring 'OfflineGear_ID_'! {itemInstanceId}");
                return 0;
            }

            try
            {
                uint offlineGearPersistentID = uint.Parse(itemInstanceId.Substring("OfflineGear_ID_".Length));
                return offlineGearPersistentID;
            }
            catch
            {
                PWLogger.Error("Caught exception while trying to parse persistentID of PlayerOfflineGearDB from GearIDRange, which means itemInstanceId could be ill-formated");
                return 0;
            }
        }
    }
}
