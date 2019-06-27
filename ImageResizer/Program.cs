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
        //調整前:8572ms , 8602ms , 8148ms , 7846ms , 8272ms
        //調整後:筆電(i7-6500u)  3252ms , 3605ms , 2282ms , 4316ms , 3298ms
        static void Main(string[] args)
        {
            string sourcePath = Path.Combine(Environment.CurrentDirectory, "images");
            string destinationPath = Path.Combine(Environment.CurrentDirectory, "output"); ;

            ImageProcess imageProcess = new ImageProcess();

            imageProcess.Clean(destinationPath);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            imageProcess.ResizeImages(sourcePath, destinationPath, 2.0);

            sw.Stop();

            Console.WriteLine($"花費時間: {sw.ElapsedMilliseconds} ms");

            Console.ReadLine();
        }
    }
}
