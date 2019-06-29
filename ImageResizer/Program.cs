using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageResizer
{
    class Program
    {
        //測試環境:筆電 i7-6500U、20 GB DDR4 RAM 、SanDisk SD8SN8U512G1002 M2 SSD 
        //測試檔案: 67個jpg檔,總共30.2MB ,VS組建組態:Debug
        //調整前:42829ms ,  42942ms ,42712ms  =>平均42828ms
        //調整後(ThreadPool版本):28251ms , 25335ms , 29868ms =>平均27818ms ,速度提升35%
        static void Main(string[] args)
        {
            string sourcePath = Path.Combine(Environment.CurrentDirectory, "images");
            string destinationPath = Path.Combine(Environment.CurrentDirectory, "output"); ;

            ImageProcess imageProcess = new ImageProcess();
            imageProcess.Clean(destinationPath);
            imageProcess.ResizeImages(sourcePath, destinationPath, 2.0);

            Console.ReadLine();
        }
    }
}
