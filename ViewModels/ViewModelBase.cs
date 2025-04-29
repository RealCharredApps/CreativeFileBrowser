using CommunityToolkit.Mvvm.ComponentModel;

namespace CreativeFileBrowser.ViewModels;

public class ViewModelBase : ObservableObject
{
    public string SystemFilesIconLabel { get; set;} = "System Files";

}
