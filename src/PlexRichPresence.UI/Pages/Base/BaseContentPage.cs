using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlexRichPresence.UI.Pages.Base;
public abstract class BaseContentPage<ViewModelType> : ContentPage
{
    protected enum Column
    {
        Description,
        Input
    }

    protected enum Row
    {
        TextEntry
    }

    protected ViewModelType ViewModel;

    public BaseContentPage(ViewModelType viewModel)
    {
        this.BindingContext = viewModel;
        this.ViewModel = viewModel;
    }
}
