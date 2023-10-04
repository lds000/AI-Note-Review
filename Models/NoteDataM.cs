using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AI_Note_Review
{
    class NoteDataM
    {
        public int NoteID { get; set; }
        public DateTime VisitDate { get; set; }
        public int ProviderID { get; set; }

        string[] NoteSegmentTitles = { "Reason for Appointment", "History of Present Illness", "Current Medications", "Taking", "Not-Taking/PRN", "Discontinued",
            "Unknown", "Active Problem List", "Past Medical History", "Surgical History","Surgical History","Family History","Social History",
            "Hospitalization/Major Diagnostic Procedure","Review of Systems","Vital Signs","Examination","Assessments","Treatment","Follow Up",
            "Allergies","Medication Summary","Electronically signed*"
        };

        private string noteString;
        public string NoteString
        {
            get
            {
                return noteString;
            }
            set
            {
                noteString = Encryption.Decrypt(value);
                /*
                foreach (string str in NoteSegmentTitles)
                {
                    noteString.Replace(str, Environment.NewLine + str + Environment.NewLine);
                }
                */
            }
        }

        public bool Reviewed { get; set; }

    }
}
