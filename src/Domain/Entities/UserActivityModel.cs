namespace Domain.Entities
{
	public class UserActivityModel
    {
        public DateTime Date { get; set; }
        public int Students { get; set; }
        public int Teachers { get; set; }
        public int Moderators { get; set; }
    }
}
