// ClinicManagement.UI\App.xaml.cs
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

namespace ClinicManagement.UI
{
    public partial class App : Application
    {
        private delegate bool AllocConsoleDelegate();

        public App()
        {
            this.Startup += App_Startup;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                // Allocate a console so test output can be seen when running the UI project directly
                try { AllocConsole(); } catch { }

                Console.WriteLine("Searching for RunFrontendTests...");

                MethodInfo runMethod = null;

                // Search all currently loaded assemblies for a Program type with a RunFrontendTests static method
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type[] types;
                    try
                    {
                        types = asm.GetTypes();
                    }
                    catch (ReflectionTypeLoadException rtle)
                    {
                        types = rtle.Types.Where(t => t != null).ToArray();
                    }

                    foreach (var t in types)
                    {
                        if (t == null) continue;
                        if (t.Name != "Program" && (t.FullName == null || !t.FullName.EndsWith(".Program"))) continue;
                        var m = t.GetMethod("RunFrontendTests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                        if (m != null)
                        {
                            runMethod = m;
                            break;
                        }
                    }

                    if (runMethod != null) break;
                }

                if (runMethod != null)
                {
                    Console.WriteLine($"Invoking {runMethod.DeclaringType?.FullName}.{runMethod.Name}...");
                    runMethod.Invoke(null, null);
                    Console.WriteLine("RunFrontendTests completed.");
                }
                else
                {
                    Console.WriteLine("RunFrontendTests method not found. Ensure Program.RunFrontendTests exists, is static, and the assembly is referenced/loaded.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while trying to run frontend tests: " + ex);
            }
        }

        private static bool AllocConsole()
        {
            IntPtr libHandle;
            if (!NativeLibrary.TryLoad("kernel32.dll", out libHandle))
            {
                return false;
            }

            try
            {
                IntPtr proc;
                if (!NativeLibrary.TryGetExport(libHandle, "AllocConsole", out proc))
                {
                    return false;
                }

                var del = Marshal.GetDelegateForFunctionPointer<AllocConsoleDelegate>(proc);
                return del();
            }
            finally
            {
                try { NativeLibrary.Free(libHandle); } catch { }
            }
        }
    }
}