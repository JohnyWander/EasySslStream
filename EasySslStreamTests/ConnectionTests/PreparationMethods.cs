using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStreamTests.ConnectionTests
{
    internal static class PreparationMethods
    {
         static string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
         static Random rnd = new Random();
        static string GetRandomstring(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[rnd.Next(s.Length)]).ToArray());
        }

        public static void CreateRandomTestDirectory(string CurrentDir, int MinFileSizeInBytes, int MaxFileSizeInBytes, int FileCount, int Depth)
        {
            

            for (int i = 0; i <= Depth; i++)
            {
                Directory.CreateDirectory(CurrentDir);
                 for (int ii = 0; ii <= FileCount; ii++)
                 {
                    string Filename = GetRandomstring(rnd.Next(1, 10));
                    string Extension = GetRandomstring(rnd.Next(2, 3));


                    byte[] ContentsBuffer = new byte[2048];
                    FileStream writer = new FileStream(CurrentDir + "\\" + Filename + "." + Extension,FileMode.Create);
                    int DesiredFileSize = rnd.Next(MinFileSizeInBytes, MaxFileSizeInBytes);
                    rnd.NextBytes(ContentsBuffer); 

                    while(writer.Position < DesiredFileSize)
                    {
                        writer.Write(ContentsBuffer);
                       Debug.WriteLine(writer.Position);
                    }
                    writer.Dispose();
                 }
                 
                string newDir = GetRandomstring(rnd.Next(1, 10));
                CurrentDir += $"\\{newDir}";


            }
        }








    }
}

