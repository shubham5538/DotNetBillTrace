using MongoDB.Driver;
using KotBot.Models;
using MongoDB.Bson;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace KotBot.Services
{
    public class MongoService
    {
        private readonly IMongoCollection<OnlineBillItem> _billItemsCollection;
        private readonly IMongoCollection<OnlineBillMaster> _billMastersCollection;
        private readonly IMongoCollection<User> _userCollection;

        public MongoService(IConfiguration config)
        {
            var client = new MongoClient(config.GetValue<string>("MongoSettings:ConnectionString"));
            var database = client.GetDatabase(config.GetValue<string>("MongoSettings:DatabaseName"));
            _billItemsCollection = database.GetCollection<OnlineBillItem>("OnlineBillItems");
            _billMastersCollection = database.GetCollection<OnlineBillMaster>("OnlineBillMaster");
            _userCollection = database.GetCollection<User>("UserLogin");
        }

        // Register new user with hashed password and registration date
        public async Task RegisterUserAsync(User user)
        {
            user.PasswordHash = HashPassword(user.PasswordHash); // Hash password before saving
            user.RegistrationDate = DateTime.UtcNow; // Set registration date
            await _userCollection.InsertOneAsync(user);
        }

        // Get user by mobile number (previously username)
        public async Task<User> GetUserByMobileNoAsync(string mobileNo)
        {
            return await _userCollection.Find(x => x.MobileNo == mobileNo).FirstOrDefaultAsync();
        }

        // Update user (e.g., for updating FCMToken, password, etc.)
        public async Task UpdateUserAsync(User user)
        {
            var filter = Builders<User>.Filter.Eq(u => u.MobileNo, user.MobileNo);
            var update = Builders<User>.Update
                .Set(u => u.FCMToken, user.FCMToken)
                .Set(u => u.PasswordHash, user.PasswordHash);

            await _userCollection.UpdateOneAsync(filter, update);
        }

        // Check if the entered password matches the stored password hash
        public bool VerifyPassword(string enteredPassword, string storedPasswordHash)
        {
            using var sha256 = SHA256.Create();
            var enteredPasswordHash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(enteredPassword)));
            return enteredPasswordHash == storedPasswordHash;
        }

        // Hash password using SHA-256
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }

        // CRUD Operations for OnlineBillItem
        public async Task<List<OnlineBillItem>> GetBillItemsAsync() =>
            await _billItemsCollection.Find(_ => true).ToListAsync();

        public async Task<OnlineBillItem> GetBillItemAsync(int srNumber) =>
            await _billItemsCollection.Find(x => x.srNumber == srNumber).FirstOrDefaultAsync();

        public async Task CreateBillItemAsync(OnlineBillItem item) =>
            await _billItemsCollection.InsertOneAsync(item);

        public async Task UpdateBillItemAsync(int srNumber, OnlineBillItem item) =>
            await _billItemsCollection.ReplaceOneAsync(x => x.srNumber == srNumber, item);

        public async Task DeleteBillItemAsync(int srNumber) =>
            await _billItemsCollection.DeleteOneAsync(x => x.srNumber == srNumber);

        public async Task<OnlineBillItem> GetBillItemByIdAsync(string id) =>
            await _billItemsCollection.Find(x => x.Id == ObjectId.Parse(id)).FirstOrDefaultAsync();

        public async Task<List<OnlineBillItem>> GetBillItemsBySrNumberAsync(int srNumber) =>
            await _billItemsCollection.Find(x => x.srNumber == srNumber).ToListAsync();

        // CRUD Operations for OnlineBillMaster
        public async Task<List<OnlineBillMaster>> GetBillMastersAsync() =>
            await _billMastersCollection.Find(_ => true).ToListAsync();

        public async Task<OnlineBillMaster> GetBillMasterAsync(int srNumber) =>
            await _billMastersCollection.Find(x => x.srNumber == srNumber).FirstOrDefaultAsync();

        public async Task CreateBillMasterAsync(OnlineBillMaster master) =>
            await _billMastersCollection.InsertOneAsync(master);

        public async Task UpdateBillMasterAsync(int srNumber, OnlineBillMaster master) =>
            await _billMastersCollection.ReplaceOneAsync(x => x.srNumber == srNumber, master);

        public async Task DeleteBillMasterAsync(int srNumber) =>
            await _billMastersCollection.DeleteOneAsync(x => x.srNumber == srNumber);

        public async Task<OnlineBillMaster> GetBillMasterByIdAsync(string id) =>
            await _billMastersCollection.Find(x => x.Id == ObjectId.Parse(id)).FirstOrDefaultAsync();
    }
}
