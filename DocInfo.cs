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

        }
        public string PtName { get; set; }
        public string PtAge { get; set; }

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

        public List<string> ICD10s;

        public List<string> DocumentTags = new List<string>();

        /// <summary>
        /// An array where the index matches the notesectionID
        /// </summary>
        public string[] NoteSectionText;

    }
}
