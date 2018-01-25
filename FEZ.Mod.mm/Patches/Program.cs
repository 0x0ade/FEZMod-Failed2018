#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FezGame {
    class patch_Program {

        private static extern void orig_Main(string[] args);
        private static void Main(string[] args) {
            Queue<string> queue = new Queue<string>(args);
            while (queue.Count > 0) {
                string arg = queue.Dequeue();
                // TODO: Parse mod args.
            }

            orig_Main(args);
        }

        private static extern void orig_MainInternal();
        private static void MainInternal() {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            try {
                orig_MainInternal();
            } catch (Exception e) {
                Logger.Log("FEZMod", "Fatal error!");
                LogDetailed(e);
                throw;
            }
        }

        public static void LogDetailed(Exception e, string tag = null) {
            for (Exception e_ = e; e_ != null; e_ = e_.InnerException) {
                Console.WriteLine(e_.GetType().FullName + ": " + e_.Message + "\n" + e_.StackTrace);
                if (e_ is ReflectionTypeLoadException) {
                    ReflectionTypeLoadException rtle = (ReflectionTypeLoadException) e_;
                    for (int i = 0; i < rtle.Types.Length; i++) {
                        Console.WriteLine("ReflectionTypeLoadException.Types[" + i + "]: " + rtle.Types[i]);
                    }
                    for (int i = 0; i < rtle.LoaderExceptions.Length; i++) {
                        LogDetailed(rtle.LoaderExceptions[i], tag + (tag == null ? "" : ", ") + "rtle:" + i);
                    }
                }
                if (e_ is TypeLoadException) {
                    Console.WriteLine("TypeLoadException.TypeName: " + ((TypeLoadException) e_).TypeName);
                }
                if (e_ is BadImageFormatException) {
                    Console.WriteLine("BadImageFormatException.FileName: " + ((BadImageFormatException) e_).FileName);
                }
            }
        }

    }
}
