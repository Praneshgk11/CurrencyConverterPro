namespace CurrencyConverter
{
    public class ConversionRequest
    {
        public string SourceCurrency { get; set; }
        public string TargetCurrency { get; set; }
        public decimal Amount { get; set; }

    }
}
