using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace CreativeFileBrowser.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = "CFB";

}
