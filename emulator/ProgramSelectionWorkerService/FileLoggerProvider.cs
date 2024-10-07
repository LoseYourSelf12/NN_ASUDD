using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramSelectionWorkerService
{
    public class FileLoggerProvider : ILoggerProvider
    {
        string path;
        public FileLoggerProvider(string path)
        {
            this.path = path;
        }
        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(path);
        }

        public void Dispose() { }
    }
}
