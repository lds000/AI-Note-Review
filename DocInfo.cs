using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AI_Note_Review
{
    public class DocInfo
    {
        public DocInfo()
        {
            ICD10s = new List<string>();
        }
        public string PtName { get; set; }
        public int PtAgeYrs { get; set; }


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
        public List<string> ICD10s { get; set;}

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

        public string PtSex { get; set; }
        public string PtID { get; set; }
        public string Facility { get; set; }
        public string VisitDate { get; set; }
        public string Provider { get; set; }
        public string ReasonForAppt { get; set; }
        public string Allergies { get; set; }

        public string NoteHTML { get; set; }
        public string Vitals { get; set; }
        public string CC { get; set; }
        public string HPI { get; set; }
        public string CurrentMeds { get; set; }
        public string CurrentPrnMeds { get; set; }
        public string ProblemList { get; set; }
        public string ROS { get; set; }
        public string PMHx { get; set; }
        public string SocHx { get; set; }
        public string GeneralHx { get; set; }
        public  string Exam { get; set; }
        public string Treatment { get; set; }
        public string MedsStarted { get; set; }
        public string ImagesOrdered { get; set; }
        public string LabsOrdered { get; set; }
        public string Assessments { get; set; }
        public string FollowUp { get; set; }

        public string HashTags { get; set; }

        public string SurgHx { get; set; }

        public string FamHx { get; set; }

        public int VitalsSystolic { get; set; }
        public int VitalsDiastolic { get; set; }
        public int VitalsO2 { get; set; }
        public int VitalsHR { get; set; }
        public int VitalsRR { get; set; }
        public double VitalsTemp { get; set; }
        public double VitalsWt { get; set; }
        public double VitalsBMI { get; set; }


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
                if (VitalsBMI <= 0) return Brushes.Black;
                if (VitalsBMI >= 40)
                {
                    return Brushes.Red;
                }
                return null; //use TargetNullValue
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
         PtAgeYrs=0;
         PtSex="";
         PtID="";
         Facility="";
         VisitDate="";
         Provider="";
         ReasonForAppt="";
         Allergies="";
         NoteHTML="";
         Vitals="";
         CC="";
         HPI="";
         CurrentMeds="";
         ProblemList="";
         ROS="";
         PMHx="";
         SocHx="";
         GeneralHx="";
         Exam="";
         Treatment="";
         MedsStarted="";
         ImagesOrdered="";
         LabsOrdered="";
         Assessments="";
         FollowUp="";
         SurgHx="";
         FamHx="";
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

    }
}
