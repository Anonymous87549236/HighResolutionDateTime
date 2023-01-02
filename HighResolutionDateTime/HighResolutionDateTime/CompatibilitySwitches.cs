// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
// https://referencesource.microsoft.com/#mscorlib/system/compatibilityswitches.cs

namespace HighResolutionDateTime
{
    // [FriendAccessAllowed]
    internal static class CompatibilitySwitches
    {
#if FEATURE_LEGACYNETCF
        private static bool s_isAppEarlierThanWindowsPhone8;
        private static bool s_isAppEarlierThanWindowsPhoneMango;
#endif //FEATURE_LEGACYNETCF

        private static bool IsCompatibilitySwitchSet(string compatibilitySwitch)
        {
            bool? result = System.AppDomain.CurrentDomain.IsCompatibilitySwitchSet(compatibilitySwitch);
            return (result.HasValue && result.Value);
        }

        internal static void InitializeSwitches()
        {
#if FEATURE_LEGACYNETCF
            s_isAppEarlierThanWindowsPhoneMango = IsCompatibilitySwitchSet("WindowsPhone_3.7.0.0");
            s_isAppEarlierThanWindowsPhone8 = s_isAppEarlierThanWindowsPhoneMango || 
                                                IsCompatibilitySwitchSet("WindowsPhone_3.8.0.0"); 
                    
#endif //FEATURE_LEGACYNETCF
        }

        internal static bool IsAppEarlierThanWindowsPhone8
        {
            get
            {
#if FEATURE_LEGACYNETCF
                return s_isAppEarlierThanWindowsPhone8;
#else
                return false;
#endif //FEATURE_LEGACYNETCF
            }
        }
    }
}