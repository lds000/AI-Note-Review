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
    public class DocumentM : INotifyPropertyChanged
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
                ProviderSql = p;
                NotifyPropertyChanged();
            }
        }

        private SqlProvider providerSql;
        public SqlProvider ProviderSql
        {
            get
            {
                return providerSql;
            }
            set
            {
                providerSql = value;
                NotifyPropertyChanged();
            }
        }

        public int ProviderID
        {
            get { return providerID; }
            set
            {
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

        public ObservableCollection<SqlICD10SegmentVM> ICD10Segments
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

        public string ProcedureNote
        {
            get
            {
                return procedureNote;
            }
            set
            {
                procedureNote = value;
                NotifyPropertyChanged();
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

        #region property variables
        /// <summary>
        /// An array where the index matches the notesectionID
        /// </summary>
        DateTime dOB;
        private ObservableCollection<SqlICD10SegmentVM> iCD10Segments = new ObservableCollection<SqlICD10SegmentVM>();
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
        private ObservableCollection<string> documentTags = new ObservableCollection<string>();

        #endregion


    }
}
