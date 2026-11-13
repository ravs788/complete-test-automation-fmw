namespace API.Models
{

    public record class Booking
    {
        public string firstname { get; init; } = "";
        public string lastname { get; init; } = "";
        public int totalprice { get; init; }
        public bool depositpaid { get; init; }
        public BookingDates bookingdates { get; init; } = new BookingDates();
        public string additionalneeds { get; init; } = "";
    }
}
