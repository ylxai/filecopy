using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Win32;
using FileCopyUtility.Services;
using FileCopyUtility.Models;
using FileCopyUtility.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using System.Windows.Media.Animation;

namespace FileCopyUtility;

/// <summary>
/// Partial class for MainWindow - Main UI and initialization logic
/// </summary>
public partial class MainWindow : Window
{
    // Properties
    private readonly FileListManager _fileListManager;
    private readonly FileListManager _validFilesManager;
    private string _sourceFolder = string.Empty;

    // Services
    private readonly FileOperationService _fileService;
    private readonly FastFolderScanService _fastScanService;
    private readonly HighPerformanceFileService _highPerformanceService;
    private readonly MemoryOptimizedFileService _memoryOptimizedService;
    private readonly AdvancedReportingService _reportingService;
    private readonly AdvancedProgressService _progressService;
    private readonly RealTimeDashboardService _dashboardService;
    private readonly ToastNotificationService _toastService;
    private readonly PreviewService _previewService;
    private CancellationTokenSource? _cancellationTokenSource;

    // Performance tracking
    private DateTime _copyStartTime;
    private long _totalBytesToCopy;
    private long _totalBytesCopied;
    private CancellationTokenSource? _scanCancellationTokenSource;
    private PauseTokenSource? _pauseTokenSource;
    private Stopwatch _copyStopwatch = new();
    private System.Timers.Timer? _uiUpdateTimer;

    // Loading indicators
    private bool _isLoading = false;

    public MainWindow()
    {
        InitializeComponent();
        Title = "FileCopy Utility v2.0 - WPF Edition";

        // Initialize file list managers
        _fileListManager = new FileListManager();
        _validFilesManager = new FileListManager();

        // Initialize services
        _fileService = new FileOperationService();
        _fileService.ProgressChanged += FileService_ProgressChanged;
        _fileService.StatusChanged += FileService_StatusChanged;

        // Initialize high-performance services
        _highPerformanceService = new HighPerformanceFileService();
        _memoryOptimizedService = new MemoryOptimizedFileService(PerformanceSettings.AutoConfigure());

        // Initialize advanced services
        _reportingService = new AdvancedReportingService();
        _progressService = new AdvancedProgressService();
        _dashboardService = new RealTimeDashboardService();
        _toastService = new ToastNotificationService(this);
        _previewService = new PreviewService();

        // Setup high-performance event handlers
        _highPerformanceService.ProgressChanged += OnHighPerformanceProgress;
        _highPerformanceService.StatusChanged += FileService_StatusChanged;

        // Setup advanced service handlers
        _reportingService.ReportGenerated += (s, msg) => OnStatusChanged(msg);
        _dashboardService.DashboardUpdated += OnDashboardUpdated;

        _fastScanService = new FastFolderScanService();
        _fastScanService.ProgressUpdated += FastScanService_ProgressUpdated;

        // Setup UI update timer
        _uiUpdateTimer = new System.Timers.Timer(500);
        _uiUpdateTimer.Elapsed += (s, e) => UpdateElapsedTime();

        // Setup folder validation
        SetupFolderValidation();

        // Setup animations
        SetupAnimations();

        // Initialize performance tracking
        InitializePerformanceTracking();
    }

    // Event handlers will be moved to MainWindow.Events.cs
    // File operations will be moved to MainWindow.FileOperations.cs
    // UI handlers will be moved to MainWindow.UIHandlers.cs
    // Helper functions will be moved to MainWindow.Helpers.cs
}