using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ImageResizer
{

    public class ImageProcess
    {
        static String picSrcPath = "";
        static String picDestPath = "";
        static double picScale;

        /// <summary>
        /// 清空目的目錄下的所有檔案與目錄
        /// </summary>
        /// <param name="destPath">目錄路徑</param>
        public void Clean(string destPath)
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }
            else
            {
                var allImageFiles = Directory.GetFiles(destPath, "*", SearchOption.AllDirectories);

                foreach (var item in allImageFiles)
                {
                    File.Delete(item);
                }
            }
        }

        /// <summary>
        /// 進行圖片的縮放作業
        /// </summary>
        /// <param name="sourcePath">圖片來源目錄路徑</param>
        /// <param name="destPath">產生圖片目的目錄路徑</param>
        /// <param name="scale">縮放比例</param>
        public void ResizeImages(string sourcePath, string destPath, double scale)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            var allFiles = FindImages(sourcePath);

            List<ManualResetEvent> events = new List<ManualResetEvent>();

            int fileCounter = 0;
            foreach (var filePath in allFiles)
            {
                fileCounter++;

                //寫法一:等待全部Thread 都已經 Complete
                //參考 http://dotnetstep.blogspot.com/2009/01/threadpool-wait-for-all-thread-to.html
                ThreadPoolObj tpobj = new ThreadPoolObj();
                tpobj.ObjectID = fileCounter;
                tpobj.signal = new ManualResetEvent(false);
                tpobj.picSrcPath = filePath;
                tpobj.picDestPath = destPath;
                tpobj.picScale = scale;
                events.Add(tpobj.signal);
                WaitCallback callback = new WaitCallback(ThreadFunctionImageProcess);
                ThreadPool.SetMinThreads(4, 4);
                ThreadPool.SetMaxThreads(12, 12);
                ThreadPool.QueueUserWorkItem(callback, tpobj);


                //寫法=二:
                //ThreadPool.QueueUserWorkItem((state) =>
                //{
                //    Console.WriteLine("{0} on thread {1}", filePath, Thread.CurrentThread.ManagedThreadId);
                //    ResizeImagesTask(filePath, destPath, scale);
                //});

            }

            WaitForAll(events.ToArray());

            sw.Stop();

            Console.WriteLine("本次產檔數量" + fileCounter);
            Console.WriteLine($"花費時間: {sw.ElapsedMilliseconds} ms");
   

        }

        /// <summary>
        /// 進行圖片的縮放作業Thread Task
        /// </summary>
        /// <param name="filePath">圖片來源檔案路徑</param>
        /// <param name="destPath">產生圖片目的目錄路徑</param>
        /// <param name="scale">縮放比例</param>
        public void ResizeImagesTask(string filePath, string destPath, double scale)
        {
            Image imgPhoto = Image.FromFile(filePath);
            string imgName = Path.GetFileNameWithoutExtension(filePath);

            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;

            int destionatonWidth = (int)(sourceWidth * scale);
            int destionatonHeight = (int)(sourceHeight * scale);

            Bitmap processedImage = processBitmap((Bitmap)imgPhoto,
                sourceWidth, sourceHeight,
                destionatonWidth, destionatonHeight);

            string destFile = Path.Combine(destPath, imgName + ".jpg");
            processedImage.Save(destFile, ImageFormat.Jpeg);
        }


        /// <summary>
        /// 找出指定目錄下的圖片
        /// </summary>
        /// <param name="srcPath">圖片來源目錄路徑</param>
        /// <returns></returns>
        public List<string> FindImages(string srcPath)
        {
            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(srcPath, "*.png", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(srcPath, "*.jpg", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(srcPath, "*.jpeg", SearchOption.AllDirectories));
            return files;
        }


        /// <summary>
        /// 針對指定圖片進行縮放作業
        /// </summary>
        /// <param name="img">圖片來源</param>
        /// <param name="srcWidth">原始寬度</param>
        /// <param name="srcHeight">原始高度</param>
        /// <param name="newWidth">新圖片的寬度</param>
        /// <param name="newHeight">新圖片的高度</param>
        /// <returns></returns>
        Bitmap processBitmap(Bitmap img, int srcWidth, int srcHeight, int newWidth, int newHeight)
        {
            Bitmap resizedbitmap = new Bitmap(newWidth, newHeight);
            Graphics g = Graphics.FromImage(resizedbitmap);
            g.InterpolationMode = InterpolationMode.High; //https://docs.microsoft.com/zh-tw/dotnet/api/system.drawing.drawing2d.interpolationmode?view=netframework-4.8
            g.SmoothingMode = SmoothingMode.HighQuality; //https://docs.microsoft.com/zh-tw/dotnet/api/system.drawing.drawing2d.smoothingmode?view=netframework-4.8
            g.Clear(Color.Transparent);
            g.DrawImage(img,
                new Rectangle(0, 0, newWidth, newHeight),
                new Rectangle(0, 0, srcWidth, srcHeight),
                GraphicsUnit.Pixel);
            return resizedbitmap;
        }

        //http://dotnetstep.blogspot.com/2009/01/threadpool-wait-for-all-thread-to.html
        bool WaitForAll(ManualResetEvent[] events)
        {
            bool result = false;
            try
            {
                if (events != null)
                {
                    for (int i = 0; i < events.Length; i++)
                    {
                        events[i].WaitOne();
                    }
                    result = true;
                }
            }
            catch
            {
                result = false;
            }
            return result;
        }

        //http://dotnetstep.blogspot.com/2009/01/threadpool-wait-for-all-thread-to.html
        void ThreadFunctionImageProcess(object threadobj)
        {
            ThreadPoolObj obj = threadobj as ThreadPoolObj;
            if (obj != null)
            {
                //Console.WriteLine(obj.ObjectID.ToString());

                Console.WriteLine("{0} on thread {1}", obj.picSrcPath, Thread.CurrentThread.ManagedThreadId);
                ResizeImagesTask(obj.picSrcPath, obj.picDestPath, obj.picScale);

                obj.signal.Set();
            }
        }

    }

    //http://dotnetstep.blogspot.com/2009/01/threadpool-wait-for-all-thread-to.html
    class ThreadPoolObj
    {
        public int ObjectID;
        public ManualResetEvent signal;

        public String picSrcPath;
        public String picDestPath;
        public double picScale;
    }

}
