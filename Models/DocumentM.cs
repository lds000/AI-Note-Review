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
using System.Windows.Forms;
using System.Windows.Media;

namespace AI_Note_Review
{
    public class DocumentM
    {

        public DocumentM()
        {

        }

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
                ProviderVM p = ProviderVM.SqlGetProviderByFullName(value);
                    provider = p.FullName;
                    ProviderID = p.ProviderID;
                    ProviderSql = p;
            }
        }

        private ProviderVM providerSql;
        public ProviderVM ProviderSql
        {
            get
            {
                return providerSql;
            }
            set
            {
                providerSql = value;
                
            }
        }

        public int ProviderID
        {
            get { return providerID; }
            set
            {
                providerID = value;
                

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
                
            }
        }

        public ObservableCollection<SqlICD10SegmentVM> ICD10Segments
        {
            get
            {
                return iCD10Segments;
            }
            set
            {
                iCD10Segments = value;
                
            }
        }



        #endregion

        #region Note Raw Text
        public HtmlDocument NoteHTML
        {
            get
            {
                return noteHTML;
            }
            set
            {
                noteHTML = value;
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
                
            }
        }

        public string ProcedureNote
        {
            get
            {
                return procedureNote;
            }
            set
            {
                procedureNote = value;
                
            }
        }

        public string PreventiveMed
        {
            get
            {
                return preventiveMed;
            }
            set
            {
                preventiveMed = value;
                
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
                
            }
        }
        #endregion

        #region private property variables
        /// <summary>
        /// An array where the index matches the notesectionID
        /// </summary>
        DateTime dOB;
        private ObservableCollection<SqlICD10SegmentVM> iCD10Segments;
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
        private HtmlDocument noteHTML;
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
        private string hashTags = "";
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
        #endregion


    }
}
