using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace POCConsole
{
    //http://dotnetstep.blogspot.com/2009/01/threadpool-wait-for-all-thread-to.html
    class Program2
    {
        static void Main(string[] args)
        {
            List<ManualResetEvent> events = new List<ManualResetEvent>();
            for (int i = 0; i < 100; i++)
            {
                ThreadPoolObj obj = new ThreadPoolObj();
                obj.ObjectID = i;
                obj.signal = new ManualResetEvent(false);
                events.Add(obj.signal);


                WaitCallback callback = new WaitCallback(ThreadFunctionImageProcess);
                //WaitCallback callback = new WaitCallback(ThreadFunction);
                ThreadPool.QueueUserWorkItem(callback, obj);
            }
            WaitForAll(events.ToArray());
            Console.WriteLine("Compelted");
            Console.ReadLine();
        }
        static bool WaitForAll(ManualResetEvent[] events)
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

        static void ThreadFunction(object threadobj)
        {
            ThreadPoolObj obj = threadobj as ThreadPoolObj;
            if (obj != null)
            {
                Console.WriteLine(obj.ObjectID.ToString());
                Thread.Sleep(2000); // Just Wait To Show Syncronization 
                obj.signal.Set();
            }
        }

        static void ThreadFunctionImageProcess(object threadobj)
        {
            ThreadPoolObj obj = threadobj as ThreadPoolObj;
            if (obj != null)
            {
                Console.WriteLine(obj.ObjectID.ToString());
                //Thread.Sleep(2000); // Just Wait To Show Syncronization 

                string sourcePath = Path.Combine(Environment.CurrentDirectory, "images");
                string destinationPath = Path.Combine(Environment.CurrentDirectory, "output"); ;

                ImageProcess2 imageProcess = new ImageProcess2();
                imageProcess.Clean(destinationPath);
                imageProcess.ResizeImages(sourcePath, destinationPath, 2.0);

                obj.signal.Set();
            }
        }

    }
    class ThreadPoolObj
    {
        public int ObjectID;
        public ManualResetEvent signal;
    }
}