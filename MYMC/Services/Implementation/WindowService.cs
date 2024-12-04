using System.Windows;
using MYMC.Services.Interface;
using MYMC.ViewModels;
using MYMC.Windows.Factory;

namespace MYMC.Services.Implementation;

public sealed class WindowService(IViewModelFactory viewModelFactory, IWindowFactory windowFactory) : IWindowService, IDisposable
{
    public void ShowWindow<TViewModel>(IDictionary<string, object>? parameters = null) where TViewModel : IViewModel
    {
        var hash = IViewModel.GetHash(typeof(TViewModel).Name, parameters);
        if (_openWindows.TryGetValue(hash, out var window))
        {
            window.Activate();
        }
        else
        {
            var newWindow = windowFactory.CreateWindowForViewModel<TViewModel>();
            SubscribeToWindowClosed(newWindow, hash);
            newWindow.Show();
            _openWindows.Add(hash, newWindow);
            
            if (typeof(TViewModel) == typeof(MainViewModel))
            {
                _homeWindowHash = hash;
            }
        }
        
    }

    private void SubscribeToWindowClosed(Window window, int windowHash)
    {
        window.Closed+=OnWindowClosed;
        return;
        
        void OnWindowClosed(object? _, EventArgs __)
        {
            _openWindows.Remove(windowHash);
            window.Closed -= OnWindowClosed;
            
            if (windowHash == _homeWindowHash)
            {
                Application.Current.Shutdown();
            }
        }
    }

    private readonly Dictionary<int, Window> _openWindows = [];
    private int _homeWindowHash = -1;
    
    private bool _disposed;

    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            // dispose managed state objects here
            
            // close all open windows
            foreach (var window in _openWindows.Values)
            {
                window.Close();
            }
        }
        // free unmanaged resources here and set large fields to null
        
        _disposed = true;
    }
    
    // Override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~WindowService()
    // {
    //     Dispose(false);
    // }
    
    public void Dispose()
    {
        Dispose(true);
        // GC.SuppressFinalize(this); // suppress finalization only if 'Dispose(bool disposing)' has code to free unmanaged resources
    }
}