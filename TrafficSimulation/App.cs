using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using TrafficSimulation.Windows;

namespace TrafficSimulation
{
    internal static class App
    {
        public static string AssemblyVersion
        {
            get
            {
                Version v = Assembly.GetExecutingAssembly().GetName().Version;
                return v.Major.ToString(CultureInfo.InvariantCulture) + "." + v.Minor.ToString(CultureInfo.InvariantCulture) + (v.Build != 0 || v.Revision != 0 ? ("." + v.Build.ToString(CultureInfo.InvariantCulture) + (v.Revision != 0 ? "." + v.Revision.ToString(CultureInfo.InvariantCulture) : "")) : "");
            }
        }

        public static string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length > 0) {
                    AssemblyCopyrightAttribute copyrightAttribute = (AssemblyCopyrightAttribute)attributes[0];
                    if (!string.IsNullOrEmpty(copyrightAttribute.Copyright)) {
                        return copyrightAttribute.Copyright;
                    }
                }
                return "";
            }
        }

        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
        }
    }
}