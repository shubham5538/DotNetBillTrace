using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KotBot.Models
{
    public class OnlineBillItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }
        public int srNumber { get; set; }
        public int billItemId { get; set; }
        public string itemName { get; set; }
        public bool isFood { get; set; }
        public double itemRate { get; set; }
        public double itemQty { get; set; }
        public double itemAmt { get; set; }
        public double CnclQty { get; set; }
        public bool isCancelled { get; set; }
        public bool isComplementory { get; set; }
        public double cgstAmt { get; set; }
        public double sgstAmt { get; set; }
        public double vatAmt { get; set; }
        public string Order_time { get; set; }
        public bool isPosted { get; set; }
    }
}
