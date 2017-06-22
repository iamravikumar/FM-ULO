namespace GSA.UnliquidatedObligations.Utility
{
    public enum LoadRowsFromDelineatedTextFormats
    {
        CommaSeparatedValues,
        PipeSeparatedValues,
        Custom,
    }

    public class LoadRowsFromDelineatedTextSettings : LoadRowsSettings
    {
        public LoadRowsFromDelineatedTextFormats Format { get; set; }
        public char FieldDelim;
        public char? QuoteChar;
        public int SkipRawRows { get; set; }
    }
}
