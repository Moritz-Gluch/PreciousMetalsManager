namespace PreciousMetalsManager.Services
{
    public interface IEditPricesDialogService
    {
        PriceEditResult? ShowEditPricesDialog(
            decimal goldPrice,
            decimal silverPrice,
            decimal platinumPrice,
            decimal palladiumPrice,
            decimal broncePrice,
            string priceUnit);
    }
}