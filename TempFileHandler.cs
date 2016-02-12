using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WebRequestor {
   public class TempFileHandler {
      public static string CreateTmpFile() {
         string fileName = string.Empty;
         try {
            fileName = Path.GetTempFileName();
            FileInfo fileInfo = new FileInfo(fileName);
            fileInfo.Attributes = FileAttributes.Temporary;
         } catch {
            throw;
         }
         return fileName;
      }
      public static void UpdateTmpFile(string tmpFile, string txtUpdate) {
         StreamWriter streamWriter = File.AppendText(tmpFile);
         try {
            streamWriter.WriteLine(txtUpdate);
         } catch {
            throw;
         } finally {
            streamWriter.Flush();
            streamWriter.Close();
         }
      }
      public static string ReadTmpFile(string tmpFile) {
         string returnString = "";
         try {
            StreamReader myReader = File.OpenText(tmpFile);
            returnString = myReader.ReadToEnd();
            myReader.Close();
         } catch {
            throw;
         }
         return returnString;
      }
      public static void DeleteTmpFile(string tmpFile) {
         File.SetAttributes(tmpFile, FileAttributes.Normal);
         try {
            FileInfo file = new FileInfo(tmpFile);
            int x = 0;
            while (IsFileLocked(file) && x <= 3) {
               Thread.Sleep(1000);
               x++;
            }
            if (File.Exists(tmpFile)) {
               File.Delete(tmpFile);               
            }
         } catch {
            throw;
         }
      }
      protected static bool IsFileLocked(FileInfo file) {
         FileStream stream = null;
         try {
            stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
         } catch (IOException) {
            //The file is unavailable because it is:
            //    still being written to,
            //    or being processed by another thread,
            //    or does not exist (has already been processed).
            return true;
         } finally {
            if (stream != null)
               stream.Close();
         }
         //The file is not locked
         return false;
      }
   }
}
