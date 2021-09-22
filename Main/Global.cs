using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using log4net;

namespace SczoneTavernDataCollector.Main
{
    public class Global
    {
        public static ILog Log = LogManager.GetLogger("Main");

        public static Version CurrentVersion = new Version(Application.ProductVersion);

        public static bool UpdateDownloading = false;
    }
}
