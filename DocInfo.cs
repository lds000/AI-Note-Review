using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public string SurgHx { get; set; }

        public string FamHx { get; set; }

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
        public string[] NoteSectionText = new string[20];

    }
}
