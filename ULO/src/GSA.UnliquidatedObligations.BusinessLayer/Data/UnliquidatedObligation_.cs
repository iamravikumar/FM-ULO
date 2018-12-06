namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
    public partial class UnliquidatedObligation
    {
        public string PdnWithInstance
            => UloHelpers.CreatePdnWithInstance(PegasysDocumentNumber, PegasysDocumentNumberInstance);
    }
}
