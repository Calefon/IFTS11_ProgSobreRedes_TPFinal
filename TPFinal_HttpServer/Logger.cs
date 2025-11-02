using System;
using System.IO;
using System.Reflection;

namespace TPHttpServer
{
    public class Logger
    {

        private string exePath = string.Empty;
        private static Logger _instance;

        private DateTime _lastCheckedDate;
        private Timer _timer;

        private Logger()
        {
            _lastCheckedDate = DateTime.Now.Date;
            _timer = new Timer(CheckForDayChange, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

            exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public void LogWrite(string logMessage)
        {
            
            try
            {
                using (StreamWriter w = File.AppendText(exePath + "\\" + _lastCheckedDate.ToString("yyyy-MM-dd") +".txt"))
                {
                    Log(logMessage, w);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void Log(string logMessage, TextWriter txtWriter)
        {
            try
            {
                txtWriter.Write("\r\nServer Log: ");
                txtWriter.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
                txtWriter.WriteLine("  :");
                txtWriter.WriteLine("  :{0}", logMessage);
                txtWriter.WriteLine("-------------------------------");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        

        private void CheckForDayChange(object state)
        {
            DateTime currentDay = DateTime.Now.Date;
            if (currentDay != _lastCheckedDate)
            {
                Console.WriteLine($"Day changed from {_lastCheckedDate.ToShortDateString()} to {currentDay.ToShortDateString()}");

                _lastCheckedDate = currentDay; // Update the last checked date
            }
        }


        public void StopMonitoring()
        {
            _timer.Dispose();
        }


        public static Logger LoggerInstance
        {
            get
            {
                //Crea la instancia solo si no existe
                if (_instance == null)
                {
                    _instance = new Logger();
                }
                return _instance;
            }
        }



    }
}