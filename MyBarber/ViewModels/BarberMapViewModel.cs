public class BarberMapViewModel
{
    public List<BarberLocationDto> Barbers { get; set; }
    public string GoogleMapsApiKey { get; set; }
}

public class BarberLocationDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Location { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Services { get; set; }
    public double? Rating { get; set; }
}