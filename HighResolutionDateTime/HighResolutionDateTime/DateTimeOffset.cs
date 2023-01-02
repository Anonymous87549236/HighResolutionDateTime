// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
// https://referencesource.microsoft.com/#mscorlib/system/datetimeoffset.cs
namespace HighResolutionDateTime
{
    public static class DateTimeOffset
    {
        // Returns a DateTimeOffset representing the current date and time.
        // The resolution of the returned value depends on the
        // HighResolutionDateTime.DateTime.
        //
        public static System.DateTimeOffset Now
        {
            get
            {
                return new System.DateTimeOffset(DateTime.Now);
            }
        }

        public static System.DateTimeOffset UtcNow
        {
            get
            {
                return new System.DateTimeOffset(DateTime.UtcNow);
            }
        }
    }
}