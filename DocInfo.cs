using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AI_Note_Review
{
    public class DocInfo : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.  
        // The CallerMemberName attribute that is applied to the optional propertyName  
        // parameter causes the property name of the caller to be substituted as an argument.  
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public DocInfo()
        {
            ICD10s = new List<string>();
        }

        private string ptName;
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

        public int ptAgeYrs;
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

        private DateTime reviewDate;
        public DateTime ReviewDate
        {
            get {
                return reviewDate; 
            }
            set
            {
                reviewDate = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("ReviewDateStr");
            }
        }

        public string ReviewDateStr
        {
            get
            {
                if (reviewDate < new DateTime(2010, 1, 1)) return "Enter Date";
                return reviewDate.ToShortDateString();
            }
        }





        DateTime _DOB;
        public DateTime DOB
        {
            get
            {
                return _DOB;
            }

            set
            {
                _DOB = value;
                PtAgeYrs = GetAgeInYears();
                NotifyPropertyChanged();
                NotifyPropertyChanged("PtAgeYrs");
                NotifyPropertyChanged("AgeStr");
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

        public List<string> iCD10s;
        public List<string> ICD10s
        {
            get
            {
                return iCD10s;
            }
            set
            {
                iCD10s = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("ICD10Segments");
            }
        }

        public List<string> ICD10Seglist
        {
            get
            {
                return new List<string>();
            }
        }

        private List<SqlICD10Segment> _ICD10Segments = new List<SqlICD10Segment>();
        public List<SqlICD10Segment> ICD10Segments
        {
            get
            {
                _ICD10Segments.Clear();
                foreach (string strICD10 in ICD10s)
                {
                    string strAlphaCode = strICD10.Substring(0, 1);
                    string str = "";
                    foreach (char ch in strICD10)
                    {
                        if (Char.IsDigit(ch)) str += ch;
                        if (ch == '.') str += ch; //preserve decimal
                        if (Char.ToLower(ch) == 'x') break; //if placeholder character, then stop.
                    }
                    double icd10numeric = double.Parse(str);

                    foreach (SqlICD10Segment ns in CF.NoteICD10Segments)
                    {
                        if (strAlphaCode == ns.icd10Chapter)
                        {
                            if (icd10numeric >= ns.icd10CategoryStart && icd10numeric <= ns.icd10CategoryEnd) _ICD10Segments.Add(ns);
                        }
                    }
                }
                return _ICD10Segments;
            }
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
        public string Facility
        {
            get
            {
                return facility;
            }
            set
            {
                facility = value;
                NotifyPropertyChanged();
            }
        }
        public string VisitDate
        {
            get
            {
                return visitDate;
            }
            set
            {
                visitDate = value;
                NotifyPropertyChanged();
            }
        }
        public string Provider
        {
            get
            {
                return provider;
            }
            set
            {
                provider = value;
                NotifyPropertyChanged();
            }
        }
        public string ReasonForAppt
        {
            get
            {
                return reasonForAppt;
            }
            set
            {
                reasonForAppt = value;
                NotifyPropertyChanged();
            }
        }
        public string Allergies
        {
            get
            {
                return allergies;
            }
            set
            {
                allergies = value;
                NotifyPropertyChanged();
            }
        }
        public string NoteHTML
        {
            get
            {
                return noteHTML;
            }
            set
            {
                noteHTML = value;
                NotifyPropertyChanged();
            }
        }
        public string Vitals
        {
            get
            {
                return vitals;
            }
            set
            {
                vitals = value;
                NotifyPropertyChanged();
            }
        }
        public string CC
        {
            get
            {
                return cC;
            }
            set
            {
                cC = value;
                NotifyPropertyChanged();
            }
        }
        public string HPI
        {
            get
            {
                return hPI;
            }
            set
            {
                hPI = value;
                NotifyPropertyChanged();
            }
        }
        public string CurrentMeds
        {
            get
            {
                return currentMeds;
            }
            set
            {
                currentMeds = value;
                NotifyPropertyChanged();
            }
        }
        public string CurrentPrnMeds
        {
            get
            {
                return currentPrnMeds;
            }
            set
            {
                currentPrnMeds = value;
                NotifyPropertyChanged();
            }
        }
        public string ProblemList
        {
            get
            {
                return problemList;
            }
            set
            {
                problemList = value;
                NotifyPropertyChanged();
            }
        }
        public string ROS
        {
            get
            {
                return rOS;
            }
            set
            {
                rOS = value;
                NotifyPropertyChanged();
            }
        }
        public string PMHx
        {
            get
            {
                return pMHx;
            }
            set
            {
                pMHx = value;
                NotifyPropertyChanged();
            }
        }
        public string SocHx
        {
            get
            {
                return socHx;
            }
            set
            {
                socHx = value;
                NotifyPropertyChanged();
            }
        }
        public string GeneralHx
        {
            get
            {
                return generalHx;
            }
            set
            {
                generalHx = value;
                NotifyPropertyChanged();
            }
        }
        public string Exam
        {
            get
            {
                return exam;
            }
            set
            {
                exam = value;
                NotifyPropertyChanged();
            }
        }
        public string Treatment
        {
            get
            {
                return treatment;
            }
            set
            {
                treatment = value;
                NotifyPropertyChanged();
            }
        }
        public string MedsStarted
        {
            get
            {
                return medsStarted;
            }
            set
            {
                medsStarted = value;
                NotifyPropertyChanged();
            }
        }
        public string ImagesOrdered
        {
            get
            {
                return imagesOrdered;
            }
            set
            {
                imagesOrdered = value;
                NotifyPropertyChanged();
            }
        }
        public string LabsOrdered
        {
            get
            {
                return labsOrdered;
            }
            set
            {
                labsOrdered = value;
                NotifyPropertyChanged();
            }
        }
        public string Assessments
        {
            get
            {
                return assessments;
            }
            set
            {
                assessments = value;
                NotifyPropertyChanged();
            }
        }
        public string FollowUp
        {
            get
            {
                return followUp;
            }
            set
            {
                followUp = value;
                NotifyPropertyChanged();
            }
        }
        public string HashTags
        {
            get
            {
                return hashTags;
            }
            set
            {
                hashTags = value;
                NotifyPropertyChanged();
            }
        }
        public string SurgHx
        {
            get
            {
                return surgHx;
            }
            set
            {
                surgHx = value;
                NotifyPropertyChanged();
            }
        }
        public string FamHx
        {
            get
            {
                return famHx;
            }
            set
            {
                famHx = value;
                NotifyPropertyChanged();
            }
        }
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
            }
        }
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

            }
        }
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
            }
        }
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
        bool isMale
        {
            get
            {
                if (PtSex.ToLower().StartsWith("m")) return true;
                return false;
            }

        }

        bool isFemale
        {
            get
            {
                if (PtSex.ToLower().StartsWith("f")) return true;
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
                    if (VitalsRR >= 18) return true;
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

        public double WtKg
        {
            get
            {
                return Math.Round((VitalsWt * 0.453592));
            }
        }


        public void Clear()
        {
            ICD10s.Clear();
            PtName = "";
            PtAgeYrs = 0;
            PtSex = "";
            PtID = "";
            Facility = "";
            VisitDate = "";
            Provider = "";
            ReasonForAppt = "";
            Allergies = "";
            NoteHTML = "";
            Vitals = "";
            CC = "";
            HPI = "";
            CurrentMeds = "";
            ProblemList = "";
            ROS = "";
            PMHx = "";
            SocHx = "";
            GeneralHx = "";
            Exam = "";
            Treatment = "";
            MedsStarted = "";
            ImagesOrdered = "";
            LabsOrdered = "";
            Assessments = "";
            FollowUp = "";
            SurgHx = "";
            FamHx = "";
        }

        /*
    * 
    Subjective:,
    Chief Complaint(s):,
    HPI:,
    Current Medication:,
    Medical History:,
    Allergies/Intolerance:,
    Surgical History:,
    Hospitalization:,
    Family History:,
    Social History:,
    ROS:,
    Objective:,
    Vitals:,
    Past Results:,
    Examination:,
    GENERAL APPEARANCE:,
    Physical Examination:,
    Assessment:,
    Assessment:,
    Plan:,
    Treatment:,
    Notes:,
    Patients tested for COVID-19 were also provided COVID-19 specific patient instructions including:,
    Procedures:,
    Immunizations:,
    Therapeutic Injections:,
    Diagnostic Imaging:,
    Lab Reports:,
    Procedure Orders:,
    Preventive Medicine:,
    PPE:,
    Next Appointment:,
    Billing Information:,
    Visit Code:,
    Procedure Codes:,
    Images:,
    */


        public List<string> DocumentTags = new List<string>();

        /// <summary>
        /// An array where the index matches the notesectionID
        /// </summary>
        public string[] NoteSectionText = new string[30];
        private string ptSex;
        private string ptID;
        private string facility;
        private string visitDate;
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
    }
}
