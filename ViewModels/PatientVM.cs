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
    public class PatientVM : INotifyPropertyChanged
    {
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private PatientM patient { get; set; }

        /// <summary>
        /// for dapper
        /// </summary>
        public PatientVM()
        {
            patient = SamplePatient;
        }

        public PatientVM(PatientM _patient)
        {
            patient = _patient;
        }

        public PatientM SamplePatient
        {
            get
            {
                PatientM spatient = new PatientM();
                //sample note - hidden with ctrl M H
                spatient.PtName = "Mark Smith";
                spatient.PtID = "618084";
                spatient.PtAgeYrs = 18;
                spatient.PtSex = "M";
                spatient.DOB = new DateTime(1969, 10, 23);
                spatient.VitalsBMI = 41;
                spatient.VitalsDiastolic = 115;
                spatient.VitalsHR = 88;
                spatient.VitalsO2 = 92;
                spatient.VitalsRR = 16;
                spatient.VitalsSystolic = 182;
                spatient.VitalsTemp = 101.2;
                spatient.VitalsWt = 194;
                return spatient;
            }
        }
    public string PtName {
            get { return patient.PtName; }
            set { patient.PtName = value; 
                OnPropertyChanged(); 
            }
        }
        public string PtID { get { return patient.PtID; } set { patient.PtID = value;  OnPropertyChanged();}  }


        public string PtSex { get { return patient.PtSex; } set { patient.PtSex = value; OnPropertyChanged(); OnPropertyChanged("isMale"); OnPropertyChanged("isFemale"); } }
        public bool isMale { get { return patient.isMale; } }
        public bool isFemale { get { return patient.isFemale; } }
        public bool IsPregCapable { get { return patient.IsPregCapable; } }


        public DateTime DOB { get { return patient.DOB; } set { patient.DOB = value; OnPropertyChanged(); OnPropertyChanged("PtAgeYrs"); OnPropertyChanged("AgeStr"); OnPropertyChanged("GetAgeInYears"); OnPropertyChanged("GetAgeInYearsDouble"); OnPropertyChanged("GetAgeInDays"); } }
        public int PtAgeYrs { get { return patient.PtAgeYrs; } set { patient.PtAgeYrs = value; OnPropertyChanged(); } }
        public string AgeStr { get { return patient.AgeStr; } }
        public int GetAgeInYears { get { return patient.GetAgeInYears(); } }
        public double GetAgeInYearsDouble { get { return patient.GetAgeInYearsDouble(); } }
        public int GetAgeInDays { get { return patient.GetAgeInDays(); } }


        public int VitalsSystolic { get { return patient.VitalsSystolic; } set { patient.VitalsSystolic = value;  OnPropertyChanged(); OnPropertyChanged("IsBPAbnormal"); OnPropertyChanged("IsBpHigh"); OnPropertyChanged("IsHTNUrgency"); OnPropertyChanged("BPColor"); }  }
        public int VitalsDiastolic { get { return patient.VitalsDiastolic; } set { patient.VitalsDiastolic = value; OnPropertyChanged(); OnPropertyChanged("IsBPAbnormal"); OnPropertyChanged("IsBpHigh"); OnPropertyChanged("IsHTNUrgency"); OnPropertyChanged("BPColor"); } }
        public bool IsBPAbnormal { get { return patient.IsBPAbnormal; } }
        public bool IsBpHigh { get { return patient.isMale; } }
        public bool IsHTNUrgency { get { return patient.IsHTNUrgency; } }
        public Brush BPColor { get { return patient.BPColor; } }


        public int VitalsO2 { get { return patient.VitalsO2; } set { patient.VitalsO2 = value; OnPropertyChanged(); OnPropertyChanged("isO2Abnormal"); OnPropertyChanged("O2Color"); }  }
        public bool isO2Abnormal { get { return patient.isO2Abnormal; } }
        public Brush O2Color { get { return patient.O2Color; } }


        public int VitalsHR { get { return patient.VitalsHR; } set { patient.VitalsHR = value; OnPropertyChanged(); OnPropertyChanged("isHRHigh"); OnPropertyChanged("isHRAbnormal"); OnPropertyChanged("HRColor"); }  }
        public bool isHRHigh { get { return patient.isHRHigh; } }
        public bool isHRAbnormal { get { return patient.isHRAbnormal; } }
        public Brush HRColor { get { return patient.HRColor; } }


        public int VitalsRR { get { return patient.VitalsRR; } set { patient.VitalsRR = value; OnPropertyChanged(); OnPropertyChanged("isRRHigh"); OnPropertyChanged("isRRAbnormal"); OnPropertyChanged("RRColor"); }  }
        public bool isRRHigh { get { return patient.isRRHigh; } }
        public bool isRRAbnormal { get { return patient.isRRAbnormal; } }
        public Brush RRColor { get { return patient.RRColor; } }


        public double VitalsTemp { get { return patient.VitalsTemp; } set { patient.VitalsTemp = value; OnPropertyChanged(); OnPropertyChanged("isTempMildlyElevated"); OnPropertyChanged("isTempAbnormal"); OnPropertyChanged("isTempHigh"); OnPropertyChanged("TempColor"); }  }
        public bool isTempMildlyElevated { get { return patient.isTempMildlyElevated; } }
        public bool isTempAbnormal { get { return patient.isTempAbnormal; } }
        public bool isTempHigh { get { return patient.isTempHigh; } }
        public Brush TempColor { get { return patient.TempColor; } }


        public double VitalsWt { get { return patient.VitalsWt; } set { patient.VitalsWt = value; OnPropertyChanged(); OnPropertyChanged("VitalsBMI"); OnPropertyChanged("WtKg"); OnPropertyChanged("WtColor"); }  }
        public double VitalsBMI { get { return patient.VitalsBMI; } set { patient.VitalsBMI = value; OnPropertyChanged();}  }
        public double WtKg { get { return patient.WtKg; } }
        public Brush WtColor { get { return patient.WtColor; } }


        public void Clear()
        {
            //demographics
            patient.PtName = "";
            patient.DOB = DateTime.MinValue;
            patient.PtAgeYrs = 0;
            patient.PtSex = "";
            patient.PtID = "";
        }

    }
}
