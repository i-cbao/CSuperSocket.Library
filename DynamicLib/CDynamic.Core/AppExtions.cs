using Microsoft.Extensions.PlatformAbstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dynamic.Core
{
    public class AppExtions
    {
        private static string currentBaseDir;
        public static string CurrentBaseDirectory {
            get {
                if (string.IsNullOrEmpty(currentBaseDir))
                {
                    currentBaseDir = PlatformServices.Default.Application.ApplicationBasePath;
                }
                return currentBaseDir;
            }
        }
    }
}
