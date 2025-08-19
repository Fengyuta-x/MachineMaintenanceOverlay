using System;

namespace MachineCheck.Models
{
    public class MachineCheckRecord
    {
        public int CheckId { get; set; }
        public DateTime CheckDate { get; set; }
        public string OperatorName { get; set; }
        public byte[] ProtocolPdf { get; set; }
    }
}