namespace PrivatCurrency
{
    public class Currency
    {
        public string MainCurrency { get; set; }
        public string TranslatingCurrency { get; set; }
        public string Sell { get; set; }
        public string Buy { get; set; }
        public override string ToString()
        {
            return $"{MainCurrency}-{TranslatingCurrency}:\nПокупка {Buy}\nПродажа {Sell}";
        }
    }
}
