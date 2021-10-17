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
    public class Patient : INotifyPropertyChanged
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

        Patient patient;
        public Patient()
        {
        }

        #region Patient Demographics
        public string PtName
        {
            get
            {
                return ptName;
            }
            set
            {
                ptName = value;
                NotifyPropertyChanged();
            }
        }
        public DateTime DOB
        {
            get
            {
                return dOB;
            }

            set
            {
                dOB = value;
                PtAgeYrs = GetAgeInYears();
                NotifyPropertyChanged();
                NotifyPropertyChanged("PtAgeYrs");
                NotifyPropertyChanged("AgeStr");
            }
        }
        public int PtAgeYrs
        {
            get
            {
                return ptAgeYrs;
            }
            set
            {
                ptAgeYrs = value;
                NotifyPropertyChanged();
            }
        }
        public string AgeStr
        {
            get
            {
                if (DOB == DateTime.MinValue) return "Error";
                DateTime now = DateTime.Now;

                var days = now.Day - DOB.Day;
                if (days < 0)
                {
                    var newNow = now.AddMonths(-1);
                    days += (int)(now - newNow).TotalDays;
                    now = newNow;
                }
                var months = now.Month - DOB.Month;
                if (months < 0)
                {
                    months += 12;
                    now = now.AddYears(-1);
                }
                var years = now.Year - DOB.Year;
                if (years == 0)
                {
                    if (months == 0)
                    {
                        if (days == 1) return days.ToString() + " day";
                        return days.ToString() + " days";
                    }
                    else
                    {
                        if (months == 1) return months.ToString() + " mo";
                        return months.ToString() + " mos";
                    }
                }
                if (years == 1) return years.ToString() + "yr";
                return years.ToString() + " yrs";
            }
        }
        /// <summary>
        /// returns age in years, 200 if no age stored
        /// </summary>
        /// <returns></returns>
        public int GetAgeInYears()
        {
            DateTime tmpDOB = DOB;
            if (tmpDOB == DateTime.MaxValue) return 200;
            DateTime now = DateTime.Now;
            int age = now.Year - tmpDOB.Year;
            if (now.Month < tmpDOB.Month || (now.Month == tmpDOB.Month && now.Day < tmpDOB.Day)) age--;
            return age;
        }

        /// <summary>
        /// returns age in years, 200 if no age stored
        /// </summary>
        /// <returns></returns>
        public double GetAgeInYearsDouble()
        {
            DateTime bd = DOB;
            TimeSpan age = DateTime.Now.Subtract(bd);
            return age.TotalDays / 365.25;
        }
        /// <summary>
        /// returns age in years, 200 if no age stored
        /// </summary>
        /// <returns></returns>
        public int GetAgeInDays()
        {
            DateTime tmpDOB = DOB;
            if (tmpDOB == DateTime.MaxValue) return 20000;
            DateTime now = DateTime.Now;
            int age = (now - tmpDOB).Days;
            return age;
        }

        public string PtSex
        {
            get
            {
                return ptSex;
            }
            set
            {
                ptSex = value;
                NotifyPropertyChanged();
            }
        }
        public bool isMale
        {
            get
            {
                if (PtSex.ToLower().StartsWith("m")) return true;
                return false;
            }

        }
        public bool isFemale
        {
            get
            {
                if (PtSex.ToLower().StartsWith("f")) return true;
                return false;
            }

        }
        public bool IsPregCapable
        {
            get
            {
                if (isMale) return false;
                if (PtAgeYrs >= 10)
                {
                    if (PtAgeYrs <= 55) //this is an arbitrary age, consider older
                        return true;
                }
                return false;
            }
        }
        public string PtID
        {
            get
            {
                return ptID;
            }
            set
            {
                ptID = value;
                NotifyPropertyChanged();
            }
        }

        #endregion
        #region vitals
        //BP
        public int VitalsSystolic
        {
            get
            {
                return vitalsSystolic;
            }
            set
            {
                vitalsSystolic = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("BPColor");
                NotifyPropertyChanged("IsBPAbnormal");
                NotifyPropertyChanged("IsBpHigh");
                NotifyPropertyChanged("IsHTNUrgency");
            }
        }
        public int VitalsDiastolic
        {
            get
            {
                return vitalsDiastolic;
            }
            set
            {
                vitalsDiastolic = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("BPColor");
                NotifyPropertyChanged("IsBPAbnormal");
                NotifyPropertyChanged("IsBpHigh");
                NotifyPropertyChanged("IsHTNUrgency");
            }
        }
        public bool IsBPAbnormal
        {
            get
            {
                int AgeYr = GetAgeInYears();
                if (AgeYr <= 1)
                {
                    if (VitalsSystolic >= 100) return true;
                    if (VitalsSystolic <= 80) return true;
                    if (VitalsDiastolic >= 65) return true;
                    if (VitalsDiastolic <= 50) return true;
                }
                else if (AgeYr <= 3)
                {
                    if (VitalsSystolic >= 105) return true;
                    if (VitalsSystolic <= 90) return true;
                    if (VitalsDiastolic >= 70) return true;
                    if (VitalsDiastolic <= 55) return true;
                }
                else if (AgeYr <= 6)
                {
                    if (VitalsSystolic >= 110) return true;
                    if (VitalsSystolic <= 95) return true;
                    if (VitalsDiastolic >= 75) return true;
                    if (VitalsDiastolic <= 60) return true;
                }
                else if (AgeYr <= 12)
                {
                    if (VitalsSystolic >= 120) return true;
                    if (VitalsSystolic <= 100) return true;
                    if (VitalsDiastolic >= 75) return true;
                    if (VitalsDiastolic <= 60) return true;
                }
                else
                {
                    if (VitalsSystolic >= 140) return true;
                    if (VitalsSystolic <= 110) return true;
                    if (VitalsDiastolic >= 90) return true;
                    if (VitalsDiastolic <= 65) return true;
                }
                return false;
            }
        }
        public bool IsBpHigh
        {
            get
            {
                int AgeYr = GetAgeInYears();
                if (AgeYr <= 1)
                {
                    if (VitalsSystolic >= 100) return true;
                    if (VitalsDiastolic >= 65) return true;
                }
                else if (AgeYr <= 3)
                {
                    if (VitalsSystolic >= 105) return true;
                    if (VitalsDiastolic >= 70) return true;
                }
                else if (AgeYr <= 6)
                {
                    if (VitalsSystolic >= 110) return true;
                    if (VitalsDiastolic >= 75) return true;
                }
                else if (AgeYr <= 12)
                {
                    if (VitalsSystolic >= 120) return true;
                    if (VitalsDiastolic >= 75) return true;
                }
                else
                {
                    if (VitalsSystolic >= 140) return true;
                    if (VitalsDiastolic >= 90) return true;
                }
                return false;
            }

        }
        public Brush BPColor
        {
            get
            {
                if (VitalsSystolic <= 0 || VitalsDiastolic <= 0)
                {
                    return Brushes.Black;
                }
                Brush bReturn = null; //use TargetNullValue
                if (IsBPAbnormal)
                {
                    if (IsBpHigh)
                    {
                        bReturn = Brushes.Yellow;
                    }
                    else
                    {
                        bReturn = Brushes.DodgerBlue;
                    }
                    if (VitalsSystolic >= 180 || VitalsDiastolic >= 110)
                    {
                        bReturn = Brushes.Red;
                    }
                }
                return bReturn;
            }
        }
        public bool IsHTNUrgency
        {
            get
            {
                if (VitalsSystolic >= 180) return true;
                if (VitalsDiastolic >= 120) return true;
                return false;
            }
        }

        //O2
        public int VitalsO2
        {
            get
            {
                return vitalsO2;
            }
            set
            {
                vitalsO2 = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("O2Color");
                NotifyPropertyChanged("isO2Abnormal");
            }
        }
        public Brush O2Color
        {
            get
            {
                if (VitalsO2 <= 0) return Brushes.Black;
                if (isO2Abnormal)
                {
                    return Brushes.Red;
                }
                return null; //use TargetNullValue
            }
        }
        public bool isO2Abnormal
        {
            get
            {
                if (VitalsO2 <= 92) return true;
                return false;
            }
        }

        //HR
        public int VitalsHR
        {
            get
            {
                return vitalsHR;
            }
            set
            {
                vitalsHR = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("HRColor");
                NotifyPropertyChanged("isHRHigh");
                NotifyPropertyChanged("isHRAbnormal");

            }
        }
        public Brush HRColor
        {
            get
            {
                if (VitalsHR <= 0) return Brushes.Black;
                if (isHRAbnormal)
                {
                    return Brushes.Red;
                }
                return null; //use TargetNullValue;
            }
        }
        public bool isHRHigh
        {
            get
            {
                int age = GetAgeInYears();
                if (age <= 1)
                {
                    if (VitalsHR >= 160) return true;
                }
                else if (age <= 2)
                {
                    if (VitalsHR >= 150) return true;
                }
                else if (age <= 5)
                {
                    if (VitalsHR >= 140) return true;
                }
                else if (age <= 12)
                {
                    if (VitalsHR >= 120) return true;
                }
                else
                {
                    if (VitalsHR >= 100) return true;
                }
                return false;
            }
        }
        public bool isHRAbnormal
        {
            get
            {
                int age = GetAgeInYears();
                if (age <= 1)
                {
                    if (VitalsHR >= 160) return true;
                    if (VitalsHR <= 100) return true;

                }
                else if (age <= 2)
                {
                    if (VitalsHR >= 150) return true;
                    if (VitalsHR <= 90) return true;

                }
                else if (age <= 5)
                {
                    if (VitalsHR >= 140) return true;
                    if (VitalsHR <= 80) return true;

                }
                else if (age <= 12)
                {
                    if (VitalsHR >= 120) return true;
                    if (VitalsHR <= 70) return true;

                }
                else
                {
                    if (VitalsHR >= 100) return true;
                    if (VitalsHR <= 60) return true;
                }
                return false;
            }
        }

        //RR
        public int VitalsRR
        {
            get
            {
                return vitalsRR;
            }
            set
            {
                vitalsRR = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("isRRHigh");
                NotifyPropertyChanged("isRRAbnormal");
                NotifyPropertyChanged("RRColor");
            }
        }
        public Brush RRColor
        {
            get
            {
                if (VitalsRR <= 0) return Brushes.Black;
                if (isRRAbnormal)
                {
                    return Brushes.Red;
                }
                return Brushes.White;
            }
        }
        public bool isRRHigh
        {
            get
            {
                int age = GetAgeInYears();
                if (age <= 1)
                {
                    if (VitalsRR >= 40) return true;
                }
                else if (age <= 3)
                {
                    if (VitalsRR >= 30) return true;
                }
                else if (age <= 6)
                {
                    if (VitalsRR >= 25) return true;
                }
                else if (age <= 12)
                {
                    if (VitalsRR >= 22) return true;
                }
                else
                {
                    if (VitalsRR >= 20) return true; //some suggest 18, but for document review purposes I want to be sure it is abnormal so 20
                }
                return false;
            }
        }
        public bool isRRAbnormal
        {
            get
            {
                int age = GetAgeInYears();
                if (age <= 1)
                {
                    if (VitalsRR >= 40) return true;
                    if (VitalsRR <= 25) return true;

                }
                else if (age <= 3)
                {
                    if (VitalsRR >= 30) return true;
                    if (VitalsRR <= 20) return true;

                }
                else if (age <= 6)
                {
                    if (VitalsRR >= 25) return true;
                    if (VitalsRR <= 20) return true;

                }
                else if (age <= 12)
                {
                    if (VitalsRR >= 22) return true;
                    if (VitalsRR <= 14) return true;

                }
                else
                {
                    if (VitalsRR >= 18) return true;
                    if (VitalsRR <= 12) return true;
                }
                return false;
            }
        }

        //Temp
        public double VitalsTemp
        {
            get
            {
                return vitalsTemp;
            }
            set
            {
                vitalsTemp = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("TempColor");
                NotifyPropertyChanged("isTempMildlyElevated");
                NotifyPropertyChanged("isTempAbnormal");
            }
        }
        public Brush TempColor
        {
            get
            {
                if (VitalsTemp <= 0) return Brushes.Black;
                Brush bReturn = null; //use TargetNullValue
                if (isTempAbnormal)
                {
                    bReturn = Brushes.Red;
                }
                if (isTempMildlyElevated)
                {
                    bReturn = Brushes.Yellow;
                }
                return bReturn;
            }
        }
        public bool isTempMildlyElevated
        {
            get
            {
                if (VitalsTemp >= 99) return true;
                return false;
            }
        }
        public bool isTempAbnormal
        {
            get
            {
                if (VitalsTemp >= 100.4) return true;
                return false;
            }
        }

        public bool isTempHigh
        {
            get
            {
                if (VitalsTemp >= 102) return true;
                return false;
            }
        }

        //Wt
        public double VitalsWt
        {
            get
            {
                return vitalsWt;
            }
            set
            {
                vitalsWt = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("WtColor");
                NotifyPropertyChanged("VitalsBMI");
                NotifyPropertyChanged("WtKg");

            }
        }
        public double VitalsBMI
        {
            get
            {
                return vitalsBMI;
            }
            set
            {
                vitalsBMI = value;
                NotifyPropertyChanged();
            }
        }
        public Brush WtColor
        {
            get
            {
                if (VitalsBMI < 0) return Brushes.Black;
                if (VitalsBMI >= 40)
                {
                    return Brushes.Red;
                }
                return Brushes.White; //use TargetNullValue
            }
        }
        public double WtKg
        {
            get
            {
                return Math.Round((VitalsWt * 0.453592));
            }
        }

        #endregion

        #region property variables
        /// <summary>
        /// An array where the index matches the notesectionID
        /// </summary>
        DateTime dOB;
        private ObservableCollection<SqlICD10Segment> iCD10Segments = new ObservableCollection<SqlICD10Segment>();
        public string[] NoteSectionText = new string[30];
        public int ptAgeYrs;
        private int providerID;
        private string ptName;
        private string ptSex;
        private string ptID;
        private string facility;
        private DateTime visitDate;
        private string provider;
        private string reasonForAppt;
        private string allergies;
        private string noteHTML;
        private string vitals;
        private string cC;
        private string hPI;
        private string currentMeds;
        private string currentPrnMeds;
        private string problemList;
        private string rOS;
        private string pMHx;
        private string socHx;
        private string generalHx;
        private string exam;
        private string treatment;
        private string procedureNote;
        private string preventiveMed;
        private string medsStarted;
        private string imagesOrdered;
        private string labsOrdered;
        private string assessments;
        private string followUp;
        private string hashTags;
        private string surgHx;
        private string famHx;
        private int vitalsSystolic;
        private int vitalsDiastolic;
        private int vitalsO2;
        private int vitalsHR;
        private int vitalsRR;
        private double vitalsTemp;
        private double vitalsWt;
        private double vitalsBMI;
        private string visitCodes;
        private ObservableCollection<string> iCD10s = new ObservableCollection<string>();
        private ObservableCollection<string> documentTags = new ObservableCollection<string>();

        #endregion

    }
}
