using Dapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace AI_Note_Review
{
    public class NoteHunterM: INotifyPropertyChanged
    {

        #region inotify

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.  
        // The CallerMemberName attribute that is applied to the optional propertyName  
        // parameter causes the property name of the caller to be substituted as an argument.  
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            //Console.WriteLine($"iNotify property {propertyName}");
        }

        #endregion

        private DocumentVM foundDocumentVM;
        public DocumentVM FoundDocumentVM { get; set; }

        public void SetDay(DateTime dt)
        {
            /*
            AutoIt.AutoItX.MouseMove(600, 230);
            Thread.Sleep(2000);
            AutoIt.AutoItX.MouseMove(600, 855);
            Thread.Sleep(2000);
            this.Close();
            */
            Console.WriteLine("sending keys");

            int i = AutoIt.AutoItX.WinActivate("eClinicalWorks (");
            AutoIt.AutoItX.MouseClick("LEFT", 600, 190, 2); //click month

            //send date
            AutoIt.AutoItX.Send(dt.Month.ToString());
            Thread.Sleep(200);
            AutoIt.AutoItX.MouseClick("LEFT", 617, 190, 2); //click day
            AutoIt.AutoItX.Send(dt.Day.ToString());
            Thread.Sleep(200);
            AutoIt.AutoItX.MouseClick("LEFT", 640, 190, 2); //click year
            AutoIt.AutoItX.Send(dt.Year.ToString());
            Thread.Sleep(2000); //wait for encounters to load

            //get encounters information from Copy (F2) function
            AutoIt.AutoItX.Send("{F2}");
            Thread.Sleep(200);
            AutoIt.AutoItX.WinActivate("CwReport");
            Thread.Sleep(200);
            AutoIt.AutoItX.Send("^a");
            Thread.Sleep(200);
            AutoIt.AutoItX.Send("^c");
            Thread.Sleep(200);
            string strVisitReport = AutoIt.AutoItX.ClipGet();
            AutoIt.AutoItX.Send("^w");

            List<DocumentVM> htmlDocs = new List<DocumentVM>();

            //process visits
            using (StringReader reader = new StringReader(strVisitReport))
            {
                string line;
                int VisitCount = 0;
                int offset = 217; //Where the first encounter starts (y-offset)
                double iHeight = (double)(860 - 217) / (double)40; //Height of 40 encounters
                int y = 0; //counter for lines
                while (null != (line = reader.ReadLine())) //read line by line to end
                {
                    int linecount = line.Split('\t').Count(); //tab delimited file
                    {
                        if (y <= 47) //only do top 47, rest is off screen.. later consider clicking on lower 1/2
                        {
                            string[] strArray = line.Split('\t');
                            string strVisitType = strArray[2];
                            if (strVisitType == "UC" || strVisitType == "Resp") //only respiratory or UC visits
                                if (strArray[7].ToLower().Contains("sore"))
                                {
                                    VisitCount++;
                                    AutoIt.AutoItX.MouseClick("LEFT", 1900, 235); //reset the horizontal slide bar
                                    AutoIt.AutoItX.MouseClick("LEFT", 580, offset + (int)(iHeight * (double)y), 2); //double click encounter to open, this needs long delay
                                    Console.WriteLine($"Clicked on visit. CC: {strArray[7]}");
                                    int counter = 1;
                                    while (counter <= 5)
                                    {
                                        AutoIt.AutoItX.MouseClick("LEFT", 1200, 300); //blank space on encounter note, to load note
                                        IntPtr hwnd = AutoIt.AutoItX.WinGetHandle();
                                        Thread.Sleep(1000);
                                        HookIE h = new HookIE(hwnd, 0);
                                        if (h.EcwHTMLDocument.Body.InnerHtml != null)
                                        {
                                            DocumentVM d = new DocumentVM(h.EcwHTMLDocument);
                                            if (MasterReviewSummaryVM.CurrentMasterReview.ContainsDocument(d))
                                            {
                                                htmlDocs.Add(d);
                                            }
                                            counter = 6;
                                        }
                                        else
                                        {
                                            counter++;
                                            Console.WriteLine($"{counter}/5 seconds no document loaded.");
                                            Thread.Sleep(1000);
                                        }
                                    }
                                    AutoIt.AutoItX.MouseClick("LEFT", 1620, 70); //Encounters bean
                                    Thread.Sleep(5000);
                                    //Thread.Sleep(100);
                                }
                        }
                    }
                    y++;
                }
                Console.WriteLine($"Visit count is: {VisitCount}");
            }

            Console.WriteLine($"done: {i}");
        }


    }
}
