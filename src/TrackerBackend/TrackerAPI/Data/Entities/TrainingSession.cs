namespace TrackerAPI.Data.Entities
{
    public class TrainingSession
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public User? User { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double DistanceMeters { get; set; }
        // Трек будем хранить как текст в формате WKT (Well-Known Text) или GeoJSON
        public string? RouteGeometryWkt { get; set; }
    }
}
