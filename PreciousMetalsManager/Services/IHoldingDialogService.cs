using PreciousMetalsManager.Models;
using PreciousMetalsManager.ViewModels;

namespace PreciousMetalsManager.Services
{
    public interface IHoldingDialogService
    {
        HoldingDialogResult ShowAddDialog(ViewModel viewModel);
        HoldingDialogResult ShowEditDialog(ViewModel viewModel, MetalHolding holding);
    }
}