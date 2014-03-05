using System;
using System.IO;
using System.Windows.Media.Imaging;
using System.Reflection;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace MatchWorkset.StartUp
{
    public static class FileLocations
    {
        //AddInDirectory is initialized at runtime
        public static String AddInDirectory;
        public static String AssemblyName;
        public static readonly String imperialTemplateDirectory = @"C:\ProgramData\Autodesk\RVT 2014\Family Templates\English_I\";
        public static readonly String ResourceNameSpace = @"MatchWorkset.Resources";
    }

    public class StartUpApp : IExternalApplication
    {
        Result IExternalApplication.OnStartup(UIControlledApplication application)
        {
            //initialize AssemblyName using reflection
            FileLocations.AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            //initialize AddInDirectory. The addin should be stored in a directory named after the assembly
            FileLocations.AddInDirectory = application.ControlledApplication.AllUsersAddinsLocation + "\\" + FileLocations.AssemblyName + "\\";


            return Result.Succeeded;
        }

        Result IExternalApplication.OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
