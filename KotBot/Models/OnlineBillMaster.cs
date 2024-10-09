using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KotBot.Models
{
    public class OnlineBillMaster
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }
        public int srNumber { get; set; }

        public int billNo { get; set; }
        public string tblno { get; set; }
        public string waitorNm { get; set; }
        public int Pax { get; set; }
        public double saleAmt { get; set; }
        public double discFood { get; set; }
        public double discBar { get; set; }
        public double serviceCharge { get; set; }
        public double taxAmt { get; set; }
        public double roundOff { get; set; }
        public double CompliAmt { get; set; }
        public double billAmt { get; set; }
        public bool isPrinted { get; set; }
        public bool isSettled { get; set; }
        public bool isCancelled { get; set; }
        public bool isComplementory { get; set; }
        public double Cash { get; set; }
        public double Card { get; set; }
        public double Credit { get; set; }
        public double Gpay { get; set; }
        public double RmTr { get; set; }
        public string StartTime { get; set; }
        public bool isPosted { get; set; }
        public DateTime EndTime { get; set; }
    }
}
