namespace DataVisualizer;

public record struct WindData(float time, float windspeed)
{
    public override string ToString()
    {
        return $"Time:{time}\nWindSpeed:{windspeed}";
    }
}
