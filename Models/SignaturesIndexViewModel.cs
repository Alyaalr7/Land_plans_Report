using Land_plans_Report.Data;
using System.Security.Cryptography.Xml;
using Land_plans_Report.Models;
using Signature = Land_plans_Report.Data.Signature;

namespace Land_plans_Report.Models
{
    // لوحة التواقيع والأختام
    public class SignaturesIndexViewModel
    {
        public Signature? ActiveStamp { get; set; }
        public int StampHistoryCount { get; set; }
        public List<User> Users { get; set; } = new();                         // المستخدمون الفعّالون اللي يوقّعون
        public Dictionary<int, Signature> ActiveSignatures { get; set; } = new(); // UserId → توقيعه الفعّال
    }
}
