namespace TrackerAPI.DTOs
{
    public class TrainingSessionDto
    {
        public Guid Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double DistanceMeters { get; set; }
        public string? RouteGeometryWkt { get; set; }
    }
}
