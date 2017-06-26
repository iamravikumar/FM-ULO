using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace GSA.UnliquidatedObligations.Utility
{
    public static class Stuff
    {
        public const string Qbert = "@!#?@!";

        public static CultureInfo CultureThis = CultureInfo.CurrentCulture;

        /// <summary>
        /// Does nothing.  It is simply used as a line where one can set breakpoints
        /// </summary>
        /// <param name="args">Pass in parameters if you don't want them compiled out</param>
        [Conditional("DEBUG")]
        public static void Noop(params object[] args)
        {
        }

        public static string Object2String(object o, string nullValue=null)
        {
            return o == null ? nullValue : o.ToString();
        }

        public static void FileTryDelete(string fn)
        {
            if (string.IsNullOrEmpty(fn)) return;
            try
            {
                File.Delete(fn);
            }
            catch (Exception) { }
        }

        public static TResult ExecuteSynchronously<TResult>(this Task<TResult> task)
        {
            var t = Task.Run(async () => await task);
            t.Wait();
            if (t.IsFaulted) throw task.Exception;
            return t.Result;
        }

        public static void ExecuteSynchronously(this Task task)
        {
            var t = Task.Run(async () => await task);
            t.Wait();
            if (t.IsFaulted) throw task.Exception;
        }

    }
}
