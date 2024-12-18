using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sidekick.Common.Ui.Views;
using Sidekick.Wpf.Services;
using Microsoft.Web.WebView2.Core;
using System.IO;
using System.Threading.Tasks;

namespace Sidekick.Wpf;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, IDisposable, INotifyPropertyChanged
{
    private readonly WpfViewLocator viewLocator;
    private readonly ILogger<MainWindow> logger;
    private bool isClosing;
    private bool webViewInitialized;
    private string? webView2RuntimePath;

    public event PropertyChangedEventHandler? PropertyChanged;

    private IServiceScope Scope { get; set; }

    public Guid Id { get; set; }

    public string? WebView2RuntimePath
    {
        get => webView2RuntimePath;
        set
        {
            if (webView2RuntimePath != value)
            {
                webView2RuntimePath = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WebView2RuntimePath)));
            }
        }
    }

    public MainWindow(WpfViewLocator viewLocator)
    {
        try
        {
            this.viewLocator = viewLocator;
            Scope = App.ServiceProvider.CreateScope();
            logger = Scope.ServiceProvider.GetRequiredService<ILogger<MainWindow>>();
            
            logger.LogDebug("[MainWindow] Creating service scope");
            Resources.Add("services", Scope.ServiceProvider);

            // Try to find WebView2 runtime path
            try
            {
                var runtimePath = CoreWebView2Environment.GetAvailableBrowserVersionString();
                if (!string.IsNullOrEmpty(runtimePath))
                {
                    WebView2RuntimePath = Path.GetDirectoryName(runtimePath);
                    logger.LogInformation("[MainWindow] Found WebView2 runtime at: {0}", WebView2RuntimePath);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "[MainWindow] Could not determine WebView2 runtime path");
            }
            
            logger.LogDebug("[MainWindow] Initializing component");
            InitializeComponent();

            // Add window loaded event handler
            Loaded += MainWindow_Loaded;
            
            // Add source initialized handler
            SourceInitialized += MainWindow_SourceInitialized;
            
            logger.LogInformation("[MainWindow] Window initialized successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[MainWindow] Error initializing MainWindow");
            System.Windows.MessageBox.Show($"Error initializing MainWindow: {ex.Message}\n\nStack trace:\n{ex.StackTrace}", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }

    private void MainWindow_SourceInitialized(object? sender, EventArgs e)
    {
        try
        {
            logger.LogDebug("[MainWindow] Window source initialized");
            
            // Ensure WebView is created
            if (WebView == null)
            {
                logger.LogError("[MainWindow] WebView control is null after source initialization");
                return;
            }

            // Set up WebView error handler
            if (WebView.WebView != null)
            {
                WebView.WebView.CoreWebView2InitializationCompleted += (s, e) =>
                {
                    if (!e.IsSuccess)
                    {
                        logger.LogError(e.InitializationException, "[MainWindow] WebView2 initialization failed");
                    }
                };
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[MainWindow] Error in source initialized handler");
        }
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            logger.LogDebug("[MainWindow] Window loaded event triggered");

            // Initialize WebView2 after the window is loaded
            await Task.Delay(500); // Give the window time to fully initialize
            await InitializeWebViewAsync();

            if (!webViewInitialized)
            {
                logger.LogWarning("[MainWindow] Window loaded but WebView is not initialized yet");
            }
            else
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[MainWindow] Error in window loaded event handler");
            System.Windows.MessageBox.Show($"Error in window loaded event: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task InitializeWebViewAsync()
    {
        try
        {
            logger.LogDebug("[MainWindow] Starting WebView initialization");

            if (WebView == null)
            {
                logger.LogError("[MainWindow] WebView control is null");
                throw new InvalidOperationException("WebView control is null");
            }

            // Wait for Blazor WebView to initialize its own WebView2 environment
            int retryCount = 0;
            const int maxRetries = 20; // Increase max retries
            const int delayMs = 250; // Increase delay between retries

            while (WebView.WebView == null && retryCount < maxRetries)
            {
                await Task.Delay(delayMs);
                retryCount++;
                logger.LogDebug("[MainWindow] Waiting for WebView2 initialization, attempt {0}/{1}", retryCount, maxRetries);
            }

            if (WebView.WebView == null)
            {
                logger.LogError("[MainWindow] WebView.WebView is still null after {0} attempts", retryCount);
                throw new InvalidOperationException("WebView.WebView initialization timeout");
            }

            // Wait for CoreWebView2 to be initialized by Blazor
            retryCount = 0;
            while (WebView.WebView.CoreWebView2 == null && retryCount < maxRetries)
            {
                await Task.Delay(delayMs);
                retryCount++;
                logger.LogDebug("[MainWindow] Waiting for CoreWebView2 initialization, attempt {0}/{1}", retryCount, maxRetries);
            }

            if (WebView.WebView.CoreWebView2 == null)
            {
                logger.LogError("[MainWindow] CoreWebView2 is still null after {0} attempts", retryCount);
                throw new InvalidOperationException("CoreWebView2 initialization timeout");
            }

            // Configure WebView2 settings
            try
            {
                WebView.WebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                WebView.WebView.CoreWebView2.Settings.AreDevToolsEnabled = false;
                WebView.WebView.CoreWebView2.Settings.IsStatusBarEnabled = false;
                WebView.WebView.CoreWebView2.Settings.IsZoomControlEnabled = false;

                webViewInitialized = true;
                logger.LogInformation("[MainWindow] WebView2 initialization completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[MainWindow] Error configuring WebView2 settings");
                throw;
            }

            // Set up event handlers
            try
            {
                WebView.WebView.NavigationCompleted += (s, e) =>
                {
                    if (e.IsSuccess)
                    {
                        logger.LogInformation("[MainWindow] Navigation completed successfully");
                    }
                    else
                    {
                        logger.LogError("[MainWindow] Navigation failed with WebErrorStatus: {0}", e.WebErrorStatus);
                    }
                };

                WebView.WebView.SourceChanged += (s, e) =>
                {
                    logger.LogDebug("[MainWindow] WebView source changed to: {0}", WebView.WebView.Source);
                };

                logger.LogDebug("[MainWindow] WebView event handlers attached");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[MainWindow] Error setting up WebView2 event handlers");
                throw;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[MainWindow] Error during WebView initialization");
            throw;
        }
    }

    internal SidekickView? SidekickView { get; set; }

    internal string? CurrentWebPath => WebView?.WebView?.Source?.ToString() != null ? WebUtility.UrlDecode(WebView.WebView.Source.ToString()) : null;

    public void Ready()
    {
        try
        {
            logger.LogDebug("[MainWindow] Ready() called");

            if (!webViewInitialized)
            {
                logger.LogWarning("[MainWindow] Ready() called but WebView is not initialized");
                return;
            }

            // Hide loading overlay
            LoadingOverlay.Visibility = Visibility.Collapsed;

            // Show WebView
            if (WebView != null)
            {
                WebView.Visibility = Visibility.Visible;
            }

            // The window background is transparent to avoid any flickering when opening a window. When the webview content is ready we need to set a background color. Otherwise, mouse clicks will go through the window.
            Background = (Brush?)new BrushConverter().ConvertFrom("#000000");
            Opacity = 0.01;

            CenterOnScreen();
            Activate();
            logger.LogDebug("[MainWindow] Ready() completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[MainWindow] Error in Ready()");
            throw;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        Resources.Remove("services");
        Scope.Dispose();
        viewLocator.Windows.Remove(this);
        base.OnClosed(e);
    }

    protected override async void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        OverlayContainer.Dispose();

        if (isClosing || !IsVisible || ResizeMode != ResizeMode.CanResize && ResizeMode != ResizeMode.CanResizeWithGrip || WindowState == WindowState.Maximized)
        {
            return;
        }

        try
        {
            await viewLocator.CacheProvider.Set($"view_preference_{SidekickView?.CurrentView.Key}",
                                                new ViewPreferences()
                                                {
                                                    Width = (int)ActualWidth,
                                                    Height = (int)ActualHeight,
                                                });
        }
        catch (Exception)
        {
            // If the save fails, we don't want to stop the execution.
        }

        isClosing = true;
    }

    protected override void OnDeactivated(EventArgs e)
    {
        base.OnDeactivated(e);

        if (SidekickView is
            {
                CloseOnBlur: true
            })
        {
            viewLocator.Close(SidekickView);
        }
    }

    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);

        Grid.Margin = WindowState == WindowState.Maximized ? new Thickness(0) : new Thickness(5);
    }

    private void CenterOnScreen()
    {
        // Get the window's handle
        var windowHandle = new WindowInteropHelper(this).Handle;

        // Get the screen containing the window
        var currentScreen = Screen.FromHandle(windowHandle);

        // Get the working area of the screen (excluding taskbar, DPI-aware)
        var workingArea = currentScreen.WorkingArea;

        // Get the DPI scaling factor for the monitor
        var dpi = VisualTreeHelper.GetDpi(this);

        // Convert physical pixels (from working area) to WPF device-independent units (DIPs)
        var workingAreaWidthInDips = workingArea.Width / (dpi.PixelsPerInchX / 96.0);
        var workingAreaHeightInDips = workingArea.Height / (dpi.PixelsPerInchY / 96.0);
        var workingAreaLeftInDips = workingArea.Left / (dpi.PixelsPerInchX / 96.0);
        var workingAreaTopInDips = workingArea.Top / (dpi.PixelsPerInchY / 96.0);

        // Get the actual size of the window in DIPs
        var actualWidth = Width;
        var actualHeight = Height;

        // Calculate centered position within the working area
        var left = workingAreaLeftInDips + (workingAreaWidthInDips - actualWidth) / 2;
        var top = workingAreaTopInDips + (workingAreaHeightInDips - actualHeight) / 2;

        // Set the window's position
        Left = left;
        Top = top;
    }

    private void TopBorder_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        DragMove();
    }

    // ReSharper disable All

    #region Code to make maximizing the window take the taskbar into account. https: //stackoverflow.com/questions/20941443/properly-maximizing-wpf-window-with-windowstyle-none

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        var hwndSource = PresentationSource.FromVisual(this) as HwndSource;
        if (hwndSource == null)
        {
            return;
        }

        hwndSource.AddHook(HookProc);
    }

    public static IntPtr HookProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg != WM_GETMINMAXINFO)
        {
            return IntPtr.Zero;
        }

        // We need to tell the system what our size should be when maximized. Otherwise it will
        // cover the whole screen, including the task bar.
        var mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO))!;

        // Adjust the maximized size and position to fit the work area of the correct monitor
        var monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

        if (monitor != IntPtr.Zero)
        {
            var monitorInfo = new MONITORINFO
            {
                cbSize = Marshal.SizeOf(typeof(MONITORINFO)),
            };
            GetMonitorInfo(monitor, ref monitorInfo);
            var rcWorkArea = monitorInfo.rcWork;
            var rcMonitorArea = monitorInfo.rcMonitor;
            mmi.ptMaxPosition.X = Math.Abs(rcWorkArea.Left - rcMonitorArea.Left);
            mmi.ptMaxPosition.Y = Math.Abs(rcWorkArea.Top - rcMonitorArea.Top);
            mmi.ptMaxSize.X = Math.Abs(rcWorkArea.Right - rcWorkArea.Left);
            mmi.ptMaxSize.Y = Math.Abs(rcWorkArea.Bottom - rcWorkArea.Top);
        }

        Marshal.StructureToPtr(mmi, lParam, true);

        return IntPtr.Zero;
    }

    private const int WM_GETMINMAXINFO = 0x0024;

    private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr handle, uint flags);

    [DllImport("user32.dll")]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MINMAXINFO
    {
        public POINT ptReserved;
        public POINT ptMaxSize;
        public POINT ptMaxPosition;
        public POINT ptMinTrackSize;
        public POINT ptMaxTrackSize;
    }

    #endregion Code to make maximizing the window take the taskbar into account. https: //stackoverflow.com/questions/20941443/properly-maximizing-wpf-window-with-windowstyle-none

    // ReSharper enable All

    public void Dispose()
    {
        OverlayContainer?.Dispose();
        if (WebView is IDisposable webViewDisposable)
        {
            webViewDisposable.Dispose();
        }
        else if (WebView != null)
        {
            _ = WebView.DisposeAsync().AsTask();
        }

        Scope.Dispose();
    }
}
