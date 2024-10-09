using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace KotBot.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [BsonElement("MobileNo")]
        public string MobileNo { get; set; } // This is used instead of username

        [BsonElement("PasswordHash")]
        public string PasswordHash { get; set; }

        [BsonElement("FCMToken")]
        [BsonIgnoreIfNull] // Make optional for login
        public string FCMToken { get; set; }

        [BsonElement("RestaurantId")]
        [BsonIgnoreIfNull] // Make optional for login
        public string RestaurantId { get; set; }

        [BsonElement("OutletId")]
        [BsonIgnoreIfNull] // Make optional for login
        public string OutletId { get; set; }

        [BsonElement("RegistrationDate")]
        [BsonIgnoreIfNull]
        public DateTime RegistrationDate { get; set; }
    }
}
