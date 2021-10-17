using Dapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace AI_Note_Review
{
    public class PatientViewModel : INotifyPropertyChanged
    {
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        private Patient patient;

        public PatientViewModel()
        {
            patient = new Patient();
        }

        public Patient Patient
        {
            get {
                return patient; 
            }
        }

        /// <summary>
        /// patient for testing purposes
        /// </summary>
        public Patient SamplePatient
        {
            get
            {
                patient = new Patient();
                //sample note - hidden with ctrl M H
                patient.PtName = "Mark Smith";
                patient.PtID = "618084";
                patient.PtAgeYrs = 18;
                patient.PtSex = "M";
                patient.DOB = new DateTime(1969, 10, 23);
                patient.VitalsBMI = 41;
                patient.VitalsDiastolic = 115;
                patient.VitalsHR = 88;
                patient.VitalsO2 = 92;
                patient.VitalsRR = 16;
                patient.VitalsSystolic = 182;
                patient.VitalsTemp = 101.2;
                patient.VitalsWt = 194;
                return patient;
            }
        }

        public void Clear()
        {
            //demographics
            patient.PtName = "";
            patient.DOB = DateTime.MinValue;
            patient.PtAgeYrs = 0;
            patient.PtSex = "";
            patient.PtID = "";
        }

    }
}
