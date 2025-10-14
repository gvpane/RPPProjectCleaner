using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace CleanWavFiles
{
    public static class RppTreeLister
    {
        public static List<string> GetAllRppFiles(string dir)
        {
            var result = new List<string>();
            if (!Directory.Exists(dir))
                return result;
            result.AddRange(Directory.GetFiles(dir, "*.rpp", SearchOption.TopDirectoryOnly));
            foreach (var subDir in Directory.GetDirectories(dir))
            {
                result.AddRange(GetAllRppFiles(subDir));
            }
            return result;
        }
    }
}
