public class ColumnModel
{
    public string Name { get; set; }
    public string DataType { get; set; } // "integer", "real", "char", "string", "$", "$Invl"
    public decimal? MaxMoney { get; set; } // Використовується для грошового типу $
    public IntervalModel Interval { get; set; } // Використовується для типу $Invl
}

public class IntervalModel
{
    public decimal Start { get; set; }
    public decimal End { get; set; }
}
