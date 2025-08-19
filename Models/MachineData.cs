using System;

namespace MachineCheck.Models
{
    public class MachineData
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string NrEwidencyjny { get; set; }
        public string InternalCode { get; set; }
        public string SerialNumber { get; set; }
        public DateTime BuyDate { get; set; }
        public DateTime? LastCheckDate { get; set; }
        public DateTime? FirstCheckDate { get; set; }
        public int? CheckInterval { get; set; }
        public int? DaysBeforeNotif { get; set; }
        public string NotifEmail { get; set; }
        public bool IsSaved { get; set; }
    }
}