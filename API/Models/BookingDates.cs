namespace API.Models
{
    public record class BookingDates
    {
        public string checkin { get; init; } = "";
        public string checkout { get; init; } = "";
    }
}
