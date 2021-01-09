using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;


namespace Linq.Path
{

    public static class LinqPath
    {
        public enum drive
        {
            SystemC   = 'C',
            ExternalD = 'D',
            ExternalE = 'E',
            ExternalF = 'F',
            ExternalW = 'W',
            ExternalX = 'X',
            ExternalY = 'Y',
            ExternalZ = 'Z'
        }

        public static string Path<T>(this IEnumerable<T> foldername, char drivename, params Func<T,object>[] names)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.CombinePath<T>(foldername, drivename, names);
                var output = Encoding.ASCII.GetString(ms.ToArray());
                Func<char[], string[]> splitStringByChar =
                    (splitChar) => output.Split(splitChar).Where(str => !string.IsNullOrWhiteSpace(str)).ToArray();

                var dirs = (output.Contains(";"))
                    ? splitStringByChar(new char[] {';'})
                    : splitStringByChar(new char[] {'\r', '\n'});
                try
                {
                    foreach (string dir in dirs)
                    {
                    // Create directory per string path..
                        lock (Directory.CreateDirectory(dir))
                        {
                            try
                            {
                                if (Directory.GetCreationTime(dir).Second > DateTime.Now.Second || Directory.GetCreationTime(dir).Minute > DateTime.Now.Minute)
                                {
                                    Directory.CreateDirectory(dir,
                                        new DirectorySecurity(dir, AccessControlSections.Owner));
                                }
                            }
                            catch (Exception createException)
                            {
                                string exceptionMsg = $"Exception occured - {createException}";
                                
                            }
                        }
                    }
                    return $"Creation Process complete, last created : {dirs.Last()}";
                }
                catch (System.NotSupportedException dirException)
                {
                    return $"Unsupported directory name error on creating one or more paths {dirException.Message}";
                }
                catch (Exception exception)
                {
                    return $"Other error on creating one or more paths {exception.Message}";
                }
            }
        }

        public static string Path<T>(this IEnumerable<T> foldername, params Func<T, object>[] names)
        {
            return foldername.Path<T>((char)LinqPath.drive.SystemC, names);
        }

        public static void CombinePath<T>(this Stream sourceStream, IEnumerable<T> foldername, params Func<T, object>[] names)
        {
            sourceStream.CombinePath<T>(foldername, (char)LinqPath.drive.SystemC, names);
        }

        public static void CombinePath<T>(this Stream sourceStream, IEnumerable<T> foldername, char drivename, params Func<T, object>[] names)
        {
            using (StreamWriter swr = new StreamWriter(sourceStream))
            {
                char[] drivesavail = new char[]{ 'C', 'D', 'E', 'F', 'W', 'X', 'Y', 'Z' };
                if (!drivesavail.Contains(drivename))
                {
                    drivename = (char)LinqPath.drive.SystemC;
                }

                var maindrv = string.Join(@"\", new string(drivename, 1) + new string(':', 1), string.Empty);
                foreach (T name in foldername)
                {
                    T folder = name;
                    var str = (names.Count() > 1)
                        ? ((IEnumerable<Func<T, object>>) names).Select<Func<T, object>, string>((Func<Func<T, object>, string>) (p => maindrv + p(folder).ToString() + "\\;"))
                        : ((IEnumerable<Func<T, object>>) names).Select<Func<T, object>, string>((Func<Func<T, object>, string>) (p => maindrv + p(folder).ToString() + "\\"));

                    if (names.Count() > 1)
                    {
                        swr.Write(string.Join(@";", str));
                    }
                    else
                    {
                        swr.WriteLine(string.Join(@"\", str));
                    }
                }
            }
        }
    }
}