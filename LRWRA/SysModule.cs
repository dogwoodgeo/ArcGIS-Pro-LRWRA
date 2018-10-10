using System;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.IO;
using ArcGIS.Desktop.Framework.Dialogs;

namespace LRWRA
{
    internal class SysModule : Module
    {
        private static SysModule _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static SysModule Current
        {
            get
            {
                return _this ?? (_this = (SysModule)FrameworkApplication.FindModule("LRWRA_Module"));
            }
        }

        // Method used to log any exceptions thrown by my users. Log is written to a .txt file on our LAN.
        public static void LogError(string Message, string StackTrace)
        {
            TextWriter writer = File.AppendText(@"O:\SHARE\405 - INFORMATION SERVICES\Middle Earth\ArcGIS Pro\ExceptionLog.txt");
            try
            {
                writer.WriteLine("***********************************************");
                writer.WriteLine(Environment.MachineName);// gets PC name for user that causes the excepton,
                writer.WriteLine(DateTime.Now);
                writer.WriteLine(Message);
                writer.WriteLine(StackTrace);
                writer.WriteLine();

            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            finally
            {
                writer.Close();
            }

        }
        #region Overrides
            /// <summary>
            /// Called by Framework when ArcGIS Pro is closing
            /// </summary>
            /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }

        #endregion Overrides

    }
}
