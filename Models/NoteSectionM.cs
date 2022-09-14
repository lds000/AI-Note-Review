using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_Note_Review
{
    public class NoteSectionM
    {
        public NoteSectionM()
        {
            SqlNoteSection = new SqlNoteSection();
        }

        public void InitiateSection(DocumentM dm, PatientVM p)
        {
            Document = dm;
            Patient = p;
        }

        private SqlNoteSection SqlNoteSection;
        private DocumentM Document;
        private PatientVM Patient;

        public int SectionID
        {
            get
            {
                return SqlNoteSection.SectionID;
            }
            set
            {
                if (SqlNoteSection.SectionID != value)
                {
                    SqlNoteSection.SectionID = value;
                }
            }
        }
        public string NoteSectionShortTitle
        {
            get
            {
                return SqlNoteSection.NoteSectionShortTitle;
            }
            set
            {
                if (SqlNoteSection.NoteSectionShortTitle != value)
                {
                    SqlNoteSection.NoteSectionShortTitle = value;
                }
            }
        }
        public string NoteSectionTitle
        {
            get
            {
                return SqlNoteSection.NoteSectionTitle;
            }
            set
            {
                if (SqlNoteSection.NoteSectionTitle != value)
                {
                    SqlNoteSection.NoteSectionTitle = value;
                }
            }
        }
        public int ScoreSection
        {
            get
            {
                return SqlNoteSection.ScoreSection;
            }
            set
            {
                if (SqlNoteSection.ScoreSection != value)
                {
                    SqlNoteSection.ScoreSection = value;
                }
            }
        }

        /*
From Database NoteSections:
0	Demographrics
1	History of Present Illness
2	Current Medications
3	Active Problem List
4	Past Medical History
5	Social History
6	Allergies
7	Vital Signs
8	Examination
9	Assessments
10	Treatment
11	Labs
12	Imaging
13	Review of Systems
15	Prescribed Medications
16	Family Hx
17	Surgical Hx
18	HashTags
19	Chief Complaint
20	Procedure Note
21	Preventive Medicine
 

        case 0: noteSectionContent = $"{patientVM.PtAgeYrs} Sex{patientVM.PtSex}"; break; //Demographics 
            case 1: noteSectionContent = HPI + ROS + PMHx + SocHx; break; //All obtained information
            case 2: noteSectionContent = CurrentMeds + CurrentPrnMeds; break; //CurrentMeds
            case 3: noteSectionContent = ProblemList; break; //Active Problem List
            case 4: noteSectionContent = PMHx; break; //Past Medical History
            case 5: noteSectionContent = SocHx; break; //Social History
            case 6: noteSectionContent = Allergies; break; //Allergies
            case 7: noteSectionContent = Vitals; break; //Vital Signs
            case 8: noteSectionContent = Exam; break; //Examination
            case 9: noteSectionContent = Assessments; break; //Assessments
            case 10: noteSectionContent = Treatment; break; //Treatment
            case 11: noteSectionContent = LabsOrdered; break; //Labs
            case 12: noteSectionContent = ImagesOrdered; break; //Imaging
            case 13: noteSectionContent = ROS; break; //Review of Systems
            case 14: noteSectionContent = Assessments; break; //Assessments
            case 15: noteSectionContent = MedsStarted; break; //Prescribed Medications
            case 16: noteSectionContent = FamHx; break;
            case 17: noteSectionContent = SurgHx; break;
            case 18: noteSectionContent = HashTags; break;
            case 19: noteSectionContent = CC; break;
            case 20: noteSectionContent = ProcedureNote; break;
            case 21: noteSectionContent = PreventiveMed; break;
 */

        private string noteSectionContent;
        public string NoteSectionContent
        {
            get
            {
                if (noteSectionContent != null)
                {
                    return noteSectionContent;
                }
                
                switch (SectionID)
                {
                    case 0:
                        noteSectionContent = $"{Patient.PtAgeYrs} Sex{Patient.PtSex}";
                        break; //Demographics 
                    case 1:
                        noteSectionContent = Document.CC + "\n" + Document.HPI + "\n" + Document.ROS + "\n" + Document.PMHx + "\n" + Document.SocHx;
                        break; //All obtained information
                    case 2:
                        noteSectionContent = Document.CurrentMeds + Document.CurrentPrnMeds;
                        break; //CurrentMeds
                    case 3:
                        noteSectionContent = Document.ProblemList;
                        break; //Active Problem List
                    case 4:
                        noteSectionContent = Document.PMHx;
                        break; //Past Medical History
                    case 5:
                        noteSectionContent = Document.SocHx;
                        break; //Social History
                    case 6:
                        noteSectionContent = Document.Allergies;
                        break; //Allergies
                    case 7:
                        noteSectionContent = Document.Vitals;
                        break; //Vital Signs
                    case 8:
                        noteSectionContent = Document.Exam;
                        break; //Examination
                    case 9:
                        noteSectionContent = Document.Assessments;
                        break; //Assessments
                    case 10:
                        noteSectionContent = Document.Treatment;
                        break; //Treatment
                    case 11:
                        noteSectionContent = Document.LabsOrdered;
                        break; //Labs
                    case 12:
                        noteSectionContent = Document.ImagesOrdered;
                        break; //Imaging
                    case 13:
                        noteSectionContent = Document.ROS;
                        break; //Review of Systems
                    case 14:
                        noteSectionContent = Document.Assessments;
                        break; //Assessments
                    case 15:
                        noteSectionContent = Document.MedsStarted;
                        break; //Prescribed Medications
                    case 16:
                        noteSectionContent = Document.FamHx;
                        break;
                    case 17:
                        noteSectionContent = Document.SurgHx;
                        break;
                    case 18:
                        noteSectionContent = Document.HashTags;
                        break;
                    case 19:
                        noteSectionContent = Document.CC;
                        break;
                    case 20:
                        noteSectionContent = Document.ProcedureNote;
                        break;
                    case 21:
                        noteSectionContent = Document.PreventiveMed;
                        break;
                    case 22:
                        noteSectionContent = Document.CC + "\n" +
                                             Document.HPI + "\n" +
                                             Document.ROS + "\n" +
                                             Document.PMHx + "\n" +
                                             Document.SocHx + "\n" +
                                             Document.Allergies + "\n" +
                                             Document.Assessments + "\n" +
                                             Document.CurrentMeds + "\n" +
                                             Document.CurrentPrnMeds + "\n" +
                                             Document.Exam + "\n" +
                                             Document.Facility + "\n" +
                                             Document.FamHx + "\n" +
                                             Document.FollowUp + "\n" +
                                             Document.GeneralHx + "\n" +
                                             Document.ICD10s + "\n" +
                                             Document.ImagesOrdered + "\n" +
                                             Document.LabsOrdered + "\n" +
                                             Document.MedsStarted + "\n" +
                                             Document.PreventiveMed + "\n" +
                                             Document.ProblemList + "\n" +
                                             Document.ProblemList + "\n" +
                                             Document.ProcedureNote + "\n" +
                                             Document.ReasonForAppt + "\n" +
                                             Document.SurgHx + "\n" +
                                             Document.Treatment + "\n" +
                                             Document.Vitals;
                        break;
                    default:
                        noteSectionContent = null;
                        break;
                }
                return noteSectionContent;
            }
            set
            {
                noteSectionContent = value;
            }
        }

    }
}
