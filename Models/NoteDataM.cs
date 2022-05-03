using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_Note_Review
{
    class NoteDataM
    {
        public int NoteID { get; set; }
        public DateTime VisitDate { get; set; }
        public int ProviderID { get; set; }
        public string NoteString
        { get; set; }
        public bool Reviewed { get; set; }
    }
}
