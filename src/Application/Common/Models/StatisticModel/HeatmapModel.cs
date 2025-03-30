namespace Application.Common.Models.StatisticModel;

public class HeatmapModel
{
    public int TotalActivity { get; set; }
    public string VỉewType { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
    public List<HeatmapData> Data { get; set; }
}

public class HeatmapData
{
    public string Date { get; set; }
    public int Count { get; set; }
}
