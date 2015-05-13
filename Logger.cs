using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace roslyncompiler
{
    public class Logger
    {
        public static void Log(EndPoint endPoint)
        {
            Log("{EndPoint:\"" + endPoint.EndPointUrl + "\", OperationName:\"" + endPoint.Operation + "\"}");
        }

        private static void WriteResult(Result result, StreamWriter sw)
        {
                sw.WriteLine("-------------------------------------------------------------------------------------------------------------------------");
                sw.WriteLine("controller :" + result.Controller);
                sw.WriteLine("method :" + result.Method);
                sw.WriteLine("End points and operation names : ");
                result.EndPoints.ToList()
                    .ForEach(p => sw.WriteLine(p.EndPointUrl + " and " + p.Operation));

                sw.WriteLine("Files visited :");
                result.FilesVisited.ToList()
                    .ForEach(p => sw.WriteLine(p.ClassName));
                sw.WriteLine("-------------------------------------------------------------------------------------------------------------------------");
        }

        public static void Log(Result result)
        {
            string path = @"E:\Shreedhar\ServiceInventoryList.txt";
            if (!File.Exists(path))
            {
                // Create a file to write to. 
                using (StreamWriter sw = File.CreateText(path))
                {
                    WriteResult(result, sw);
                }

                return;
            }

            // This text is always added, making the file longer over time 
            // if it is not deleted. 
            using (StreamWriter sw = File.AppendText(path))
            {
                WriteResult(result, sw);
            }
        }

        public static void Log(string endpoints)
        {
            string path = @"E:\Shreedhar\Inventory.txt";
          
            if (!File.Exists(path))
            {
                // Create a file to write to. 
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine(endpoints);
                    // sw.WriteLine("\n");
                }

                return;
            }

            // This text is always added, making the file longer over time 
            // if it is not deleted. 
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(endpoints);
            }
        }
    }
}
