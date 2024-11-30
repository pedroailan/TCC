using System.Security.Cryptography;

namespace TCC.Commons
{
    public class Notification
    {
        public Notification()
        {
            Message = Guid.NewGuid().ToString();
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            Date = DateTime.UtcNow;

            byte[] bytes = [];
            RandomNumberGenerator.Fill(bytes);
            File = bytes;
        }

        public long CalculateTime(long receivedTimestamp)
        {
            long latency = receivedTimestamp - Timestamp;
            return latency;
        }

        public string Message { get; set; }
        public long Timestamp { get; set; }
        public DateTime Date { get; set; }
        public byte[] File { get; set; }
    }
}