using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LMLogger.Model
{
    public class Logger
    {
        private string _path;
        private string _filePrefix;
        private bool Console;
        private bool TxtFile;

        public void LogInfo(object sender, string txt)
        {
            try
            {
                if (Console) System.Console.WriteLine($"{sender} : {txt}");
                if (TxtFile) WriteFile($"{DateTime.Now.ToString("hh:mm:ss")} || {sender} : {txt}");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Errore scrittura log : {ex.Message}");
            }

        }

        private void WriteFile(string txt)
        {
            // This text is always added, making the file longer over time
            // if it is not deleted.
            using (StreamWriter sw = File.AppendText(@$"{_path}\{DateTime.Today.ToString("yyyy_MM_dd")}.log"))
            {
                sw.WriteLineAsync(txt);
            }
        }

        public Logger UseConsole()
        {
            this.Console = true;
            return this;
        }
        public Logger UseTxtFile()
        {
            try
            {
                if (!Directory.Exists(_path)) Directory.CreateDirectory(_path);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Errore nella creazione della cartella.Assicurarsi di chiamare questo metodo dopo aver dichiarato la path.");
            }
            this.TxtFile = true;
            return this;
        }
        public Logger SetPath(string path)
        {
            _path = path;
            return this;
        }
    }
}
