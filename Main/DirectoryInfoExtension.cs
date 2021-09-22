using System.IO;

namespace SczoneTavernDataCollector.Main
{
    public static class DirectoryInfoExtension
    {
        public static void Empty(this DirectoryInfo directory)
        {
            foreach (FileInfo file in directory.GetFiles()) file.Delete();
            foreach (DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
        }
    }
}
