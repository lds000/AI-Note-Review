using Dapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace AI_Note_Review
{
    public class DocInfo : INotifyPropertyChanged
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


        public DocInfo()
        {
            ICD10s = new ObservableCollection<string>();
        }

        public void SetUpNote()
        {

            //add hashtags here. #Hash
            HashTags = "";
            if (PtAgeYrs > 65) HashTags += "@Elderly, ";
            if (PtSex.StartsWith("M")) HashTags += "@Male, ";
            if (PtSex.StartsWith("F")) HashTags += "@Female, ";
            if (PtAgeYrs < 4) HashTags += "@Child, ";
            if (PtAgeYrs < 2) HashTags += "@Infant, ";
            if (IsHTNUrgency) HashTags += "!HTNUrgency, ";
            if (isO2Abnormal) HashTags += "!Hypoxic, ";
            if (IsPregCapable) HashTags += "@pregnantcapable, ";
            if (PtAgeYrs >= 13) HashTags += "@sexuallyActiveAge, ";
            if (PtAgeYrs >= 16) HashTags += "@DrinkingAge, ";
            if (PtAgeYrs >= 2) HashTags += "@SpeakingAge, ";
            if (PtAgeYrs < 1) HashTags += "@Age<1, ";
            if (PtAgeYrs < 2) HashTags += "@Age<2, ";
            if (PtAgeYrs < 4) HashTags += "@Age<4, ";
            if (GetAgeInDays()<183) HashTags += "@Age<6mo, ";
            //Age>2,Age<18,Age=5,Age=6
            if (GetAgeInDays()<=90 && VitalsTemp > 100.4)
            {
                MessageBoxResult mr = MessageBox.Show($"This patient is {GetAgeInDays()} days old and has a fever of {VitalsTemp}.  Was the patient sent to an ED or appropriate workup performed?", "Infant Fever", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (mr == MessageBoxResult.No) HashTags += "#NeonteNotSentToED";
            }
            HashTags = HashTags.TrimEnd().TrimEnd(',');

            NoteSectionText[0] = $"{PtAgeYrs} Sex{PtSex}"; //Demographics 
            NoteSectionText[1] = HPI + ROS; //HPI
            NoteSectionText[2] = CurrentMeds + CurrentPrnMeds; //CurrentMeds
            NoteSectionText[3] = ProblemList; //Active Problem List
            NoteSectionText[4] = PMHx; //Past Medical History
            NoteSectionText[5] = SocHx; //Social History
            NoteSectionText[6] = Allergies; //Allergies
            NoteSectionText[7] = Vitals; //Vital Signs
            NoteSectionText[8] = Exam; //Examination
            NoteSectionText[9] = Assessments; //Assessments
            NoteSectionText[10] = Treatment; //Treatment
            NoteSectionText[11] = LabsOrdered; //Labs
            NoteSectionText[12] = ImagesOrdered; //Imaging
            NoteSectionText[13] = ROS; //Review of Systems
            NoteSectionText[14] = Assessments; //Assessments
            NoteSectionText[15] = MedsStarted; //Prescribed Medications
            NoteSectionText[16] = FamHx;
            NoteSectionText[17] = SurgHx;
            NoteSectionText[18] = HashTags;
            NoteSectionText[19] = CC;

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
            DateTime bd = new DateTime(1999, 1, 2);
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
        #region Review Details
        public DateTime ReviewDate
        {
            get
            {
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
        #endregion
        #region note details
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
        public string Provider
        {
            get
            {
                return provider;
            }
            set
            {

                SqlProvider p = SqlProvider.SqlGetProviderByFullName(value);
                provider = p.FullName;
                ProviderID = p.ProviderID;
                NotifyPropertyChanged();
            }
        }

        public int ProviderID
        {
            get { return providerID; }
            set {
                providerID = value;
                NotifyPropertyChanged();

            }
        }
        public DateTime VisitDate
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
        public ObservableCollection<string> ICD10s
        {
            get
            {
                return iCD10s;
            }
            set
            {
                iCD10s = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<SqlICD10Segment> ICD10Segments
        {
            get
            {
                return iCD10Segments;
            }
            set 
            {
                iCD10Segments = value;
                NotifyPropertyChanged();
            }
        }
        //todo: complete seglist
        public ObservableCollection<string> ICD10Seglist
        {
            get
            {
                return new ObservableCollection<string>();
            }
        }
        public ObservableCollection<string> DocumentTags
        {
            get 
            {
                return documentTags;    
            }
            set
            {
                documentTags = value;
            }
        }
        #endregion
        #region Note Raw Text
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

        public string VisitCodes
        {
            get
            {
                return visitCodes;
            }
            set
            {
                visitCodes = value;
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

        public ObservableCollection<SqlICD10Segment> GetSegments()
        {
            //get icd10 segments
            
            ObservableCollection<SqlICD10Segment> tmpICD10Segments = new ObservableCollection<SqlICD10Segment>();
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
                        if (icd10numeric >= ns.icd10CategoryStart && icd10numeric <= ns.icd10CategoryEnd)
                        {
                            tmpICD10Segments.Add(ns);
                        }
                    }
                }
            }

            if (IsHTNUrgency) tmpICD10Segments.Add(SqlLiteDataAccess.GetSegment(40)); //pull in HTNUrgencySegment
            if (isRRHigh) tmpICD10Segments.Add(SqlLiteDataAccess.GetSegment(72)); //pull in HTNUrgencySegment
            if (isTempHigh) tmpICD10Segments.Add(SqlLiteDataAccess.GetSegment(73)); //pull in HTNUrgencySegment
            if (isHRHigh) tmpICD10Segments.Add(SqlLiteDataAccess.GetSegment(74)); //pull in HTNUrgencySegment

            tmpICD10Segments.Add(SqlLiteDataAccess.GetSegment(36)); //add general segment that applies to all visits.
            return tmpICD10Segments;
        }

        public ObservableCollection<SqlCheckpoint> DroppedCheckPoints
        {
            get
            {
                return droppedCheckPoints;
            }
            set
            {
                droppedCheckPoints = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<SqlCheckpoint> PassedCheckPoints
        {
            get 
            {
                return passedCheckPoints;
            }
            set
            {
                passedCheckPoints = value;
                NotifyPropertyChanged();
            }
        }
        public ObservableCollection<SqlCheckpoint> MissedCheckPoints
        {
            get
            {
                return missedCheckPoints;
            }
            set
            {
                missedCheckPoints = value;
                NotifyPropertyChanged();
            }
        }
        public ObservableCollection<SqlCheckpoint> RelevantCheckPoints
        {
            get
            {
                return relevantCheckPoints;
            }
            set
            {
                relevantCheckPoints = value;
                NotifyPropertyChanged();
            }
        }
        public ObservableCollection<SqlCheckpoint> IrrelaventCP
        {
            get
            {
                return irrelaventCheckPoints;
            }
            set
            {
                irrelaventCheckPoints = value;
                NotifyPropertyChanged();
            }
        }

        //select ProviderID, strftime("%m-%Y", ReviewDate) as 'month-year', Count(distinct PtID) as Reviews from RelCPPRovider where strftime("%Y", ReviewDate) = "2021" AND ProviderID = 10 group by strftime("%m-%Y", ReviewDate)

        public ObservableCollection<SqlVisitReview> VisitDates
        {
            get
            {
                string sql = $"select Distinct PtID,VisitDate,ReviewDate from RelCPProvider r Where r.ProviderID = {ProviderID} and ReviewDate='{ReviewDate.ToString("yyyy-MM-dd")}' order by r.VisitDate;";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    return new ObservableCollection<SqlVisitReview>(cnn.Query<SqlVisitReview>(sql).ToList());
                }
            }
        }
        public ObservableCollection<SqlProvider> AllProviders
        {
            get
            {
                string sql = $"Select * from Providers where FullName != '' order by FullName;";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    return new ObservableCollection<SqlProvider>(cnn.Query<SqlProvider>(sql).ToList());
                }
            }
        }

        public ObservableCollection<SqlVisitReview> ReviewDates
        {
            get
            {
                string sql = $"select Distinct ReviewDate from RelCPProvider r Where r.ProviderID = {ProviderID} order by r.ReviewDate;";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {                    
                    return new ObservableCollection<SqlVisitReview>(cnn.Query<SqlVisitReview>(sql).ToList());
                }
            }
        }

        enum TagResult { Pass, Fail, FailNoCount, DropTag };

        private TagResult CheckTagRegExs(List<SqlTagRegEx> tmpTagRegExs)
        {
/*
1   Contains Any Pass
2   Contains All Pass
3   Contains None Pass
4   Contains Any Hide
5   Contains None Hide
6   Pass if Yes
7   Fail if Yes
*/
            foreach (SqlTagRegEx TagRegEx in tmpTagRegExs)
            {
                double age = GetAgeInYearsDouble();
                if (age < TagRegEx.MinAge)
                {
                    return TagResult.DropTag;
                }
                if (age > TagRegEx.MaxAge)
                {
                    return TagResult.DropTag;
                }
                if (TagRegEx.TagRegExType == 1 || TagRegEx.TagRegExType == 4 || TagRegEx.TagRegExType == 5) //Any, if one match then include tag
                {
                    bool isMatch = false;
                    foreach (string strRegEx in TagRegEx.RegExText.Split(','))
                    {
                        if (isMale && !TagRegEx.Male) return TagResult.DropTag;
                        if (!isMale && !TagRegEx.Female) return TagResult.DropTag;

                        if (NoteSectionText[TagRegEx.TargetSection] != null)
                            if (Regex.IsMatch(NoteSectionText[TagRegEx.TargetSection].ToLower(), CF.strRegexPrefix + strRegEx.Trim(), RegexOptions.IgnoreCase)) // /i is lower case directive for regex
                            {
                                isMatch = true;
                            }
                        if (isMatch == true && TagRegEx.TagRegExType == 4) return TagResult.FailNoCount; //Contains Any Hide - don't continue if type is "ANY NF" this is a stopper.
                    }
                    if (TagRegEx.TagRegExType == 5 && isMatch == false) //5   Contains None Hide
                    {
                        return TagResult.FailNoCount; //Contains none hide condition met.
                    }
                    if (!isMatch) return TagResult.Fail; //no conditions met for this one so all fail.
                }

                //todo: check the logic for the rest!

                if (TagRegEx.TagRegExType == 2) //ALL, if one not match then include tag
                {
                    foreach (string strRegEx in TagRegEx.RegExText.Split(','))
                    {
                        if (NoteSectionText[TagRegEx.TargetSection] != null)
                            if (!Regex.IsMatch(NoteSectionText[TagRegEx.TargetSection], CF.strRegexPrefix + strRegEx.Trim(), RegexOptions.IgnoreCase))
                            {
                                return TagResult.Fail; //any mismatch makes it false.
                            }
                    }
                }

                if (TagRegEx.TagRegExType == 3) //none
                {
                    foreach (string strRegEx in TagRegEx.RegExText.Split(','))
                    {
                        if (NoteSectionText[TagRegEx.TargetSection] != null)
                            if (Regex.IsMatch(NoteSectionText[TagRegEx.TargetSection], CF.strRegexPrefix + strRegEx.Trim(), RegexOptions.IgnoreCase))
                            {
                                return TagResult.Fail; //any match makes it false
                            }
                    }
                }
            }

            return TagResult.Pass;
        }

        public void GenerateReport()
        {

            documentTags.Clear();
            passedCheckPoints.Clear();
            missedCheckPoints.Clear();
            irrelaventCheckPoints.Clear();
            relevantCheckPoints.Clear();
            droppedCheckPoints.Clear();

            //todo put into database as relevant/irrelavent vs pass/fail
            int[] relType = { 5, 6, 9, 10, 12 }; //this is a cheesy short term fix
            List<int> AlreadyAddedPoints = new List<int>();

            foreach (SqlICD10Segment ns in ICD10Segments)
            {
                if (!ns.IncludeSegment) continue;
                //Console.WriteLine($"Now checking segment: {ns.SegmentTitle}");
                foreach (SqlCheckpoint cp in ns.GetCheckPoints())
                {
                    if (AlreadyAddedPoints.Contains(cp.CheckPointID)) //no need to double check
                    {
                        continue;
                    }
                    AlreadyAddedPoints.Add(cp.CheckPointID);
                    ///Console.WriteLine($"Now analyzing '{cp.CheckPointTitle}' checkpoint.");
                    TagResult trTagResult = TagResult.Pass;
                    foreach (SqlTag tagCurrentTag in cp.GetTags())
                    {
                        TagResult trCurrentTagResult;
                        List<SqlTagRegEx> tmpTagRegExs = tagCurrentTag.GetTagRegExs();
                        trCurrentTagResult = CheckTagRegExs(tmpTagRegExs);

                        if (trCurrentTagResult == TagResult.Fail || trCurrentTagResult == TagResult.FailNoCount || trCurrentTagResult == TagResult.DropTag)
                        {
                            //tag fails, no match.
                            trTagResult = trCurrentTagResult;
                            break; //if the first tag does not qualify, then do not proceed to the next tag.
                        }
                        documentTags.Add(tagCurrentTag.TagText);
                    }

                    if (trTagResult == TagResult.DropTag) //do not add to any category if droptag result.
                    {
                        cp.IncludeCheckpoint = false;
                        droppedCheckPoints.Add(cp);
                    }
                    else
                    if (trTagResult == TagResult.Pass)
                    {
                        if (relType.Contains(cp.CheckPointType))
                        {
                            cp.IncludeCheckpoint = true;
                            relevantCheckPoints.Add(cp);
                        }
                        else
                        {
                            if (ns.ICD10SegmentID != 36)
                            {
                                cp.IncludeCheckpoint = false;
                                passedCheckPoints.Add(cp); //do not include passed for All diagnosis.
                            }
                        }
                    }
                    else
                    {
                        if (relType.Contains(cp.CheckPointType) || trTagResult == TagResult.FailNoCount)
                        {
                            if (ns.ICD10SegmentID != 36)
                            {
                                cp.IncludeCheckpoint = false;
                                irrelaventCheckPoints.Add(cp); //do not include irrelevant for All diagnosis.
                            }
                        }
                        else
                        {
                            cp.IncludeCheckpoint = true;
                            missedCheckPoints.Add(cp);
                        }
                    }
                }
            }

            //now re-order checkpoints by severity
            passedCheckPoints = new  ObservableCollection< SqlCheckpoint >(passedCheckPoints.OrderByDescending(c => c.ErrorSeverity));
            missedCheckPoints = new ObservableCollection<SqlCheckpoint>(missedCheckPoints.OrderByDescending(c => c.ErrorSeverity));
            irrelaventCheckPoints = new ObservableCollection<SqlCheckpoint>(irrelaventCheckPoints.OrderByDescending(c => c.ErrorSeverity));
            relevantCheckPoints = new ObservableCollection<SqlCheckpoint>(relevantCheckPoints.OrderByDescending(c => c.ErrorSeverity));
            droppedCheckPoints = new ObservableCollection<SqlCheckpoint>(droppedCheckPoints.OrderByDescending(c => c.ErrorSeverity));

            /*
            //Generate Report
            Console.WriteLine($"Document report:");
            foreach (string strTag in documentTags)
            {
                Console.WriteLine($"\tTag: {strTag}");
            }

            Console.WriteLine($"Passed Checkpoints");
            foreach (SqlCheckpoint cp in passedCheckPoints)
            {
                Console.WriteLine($"\t{cp.CheckPointTitle}");
            }

            Console.WriteLine($"Failed Checkpoints");
            foreach (SqlCheckpoint cp in failedCheckPoints)
            {
                Console.WriteLine($"\t{cp.CheckPointTitle}");
            }

            Console.WriteLine($"Relevant Checkpoints");
            foreach (SqlCheckpoint cp in relevantCheckPoints)
            {
                Console.WriteLine($"\t{cp.CheckPointTitle}");
            }

            Console.WriteLine($"Irrelavant Checkpoints");
            foreach (SqlCheckpoint cp in irrelaventCheckPoints)
            {
                Console.WriteLine($"\t{cp.CheckPointTitle}");
            }
            */

            //DocumentTags = documentTags;
            //PassedCheckPoints = passedCheckPoints;
            //FailedCheckPoints = failedCheckPoints;
            //IrrelaventCP = irrelaventCheckPoints;
            //RelevantCheckPoints = relevantCheckPoints;

            //this doesn't seem to work, although logically it should
            NotifyPropertyChanged("DocumentTags");
            NotifyPropertyChanged("DroppedCheckPoints");
            NotifyPropertyChanged("PassedCheckPoints");
            NotifyPropertyChanged("MissedCheckPoints");
            NotifyPropertyChanged("IrrelaventCP");
            NotifyPropertyChanged("RelevantCheckPoints");
        }


        public void Clear()
        {
            //demographics
            PtName = "";
            DOB = DateTime.MinValue;
            PtAgeYrs = 0;
            PtSex = "";
            PtID = "";

            ReviewDate = DateTime.MinValue;

            Facility = "";
            VisitDate = new DateTime(2020,1,1);
            Provider = "";
            ReasonForAppt = "";
            Allergies = "";
            NoteHTML = "";
            Vitals = "";
            CC = "";
            HPI = "";
            CurrentMeds = "";
            CurrentPrnMeds = "";
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
            ObservableCollection<SqlICD10Segment> iCD10Segments = new ObservableCollection<SqlICD10Segment>();
            ICD10s.Clear();
        }





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
        private DateTime reviewDate;
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
        private string visitCodes;
        private ObservableCollection<string> iCD10s = new ObservableCollection<string>();
        private ObservableCollection<string> documentTags = new ObservableCollection<string>();
        private ObservableCollection<SqlCheckpoint> irrelaventCheckPoints = new ObservableCollection<SqlCheckpoint>();
        private ObservableCollection<SqlCheckpoint> missedCheckPoints = new ObservableCollection<SqlCheckpoint>();
        private ObservableCollection<SqlCheckpoint> relevantCheckPoints = new ObservableCollection<SqlCheckpoint>();
        private ObservableCollection<SqlCheckpoint> passedCheckPoints = new ObservableCollection<SqlCheckpoint>();
        private ObservableCollection<SqlCheckpoint> droppedCheckPoints = new ObservableCollection<SqlCheckpoint>();
        #endregion
    }
}
