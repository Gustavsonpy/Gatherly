namespace API.DTO.Rate
{
    public class UserScoreDTO
    {
        public Guid UserId { get; set; }
        public double Average { get; set; }
        public int Count { get; set; }
    }
}
