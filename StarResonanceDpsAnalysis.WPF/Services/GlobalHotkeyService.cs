using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Microsoft.Extensions.Logging;
using StarResonanceDpsAnalysis.WPF.Config;
using StarResonanceDpsAnalysis.WPF.Helpers;
using StarResonanceDpsAnalysis.WPF.ViewModels;
using KeyBinding = StarResonanceDpsAnalysis.WPF.Models.KeyBinding;

namespace StarResonanceDpsAnalysis.WPF.Services;

public sealed partial class GlobalHotkeyService(
    ILogger<GlobalHotkeyService> logger,
    IWindowManagementService windowManager,
    IConfigManager configManager,
    IMousePenetrationService mousePenetration,
    ITopmostService topmostService,
    DpsStatisticsViewModel dpsStatisticsViewModel,
    PersonalDpsViewModel personalDpsViewModel) // ? 新增: 注入个人打桩模式ViewModel
    : IGlobalHotkeyService
{
    private const int WM_HOTKEY = 0x0312;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int WH_KEYBOARD_LL = 13;
    private const int ERROR_HOTKEY_ALREADY_REGISTERED = 1409;
    private const int VK_SHIFT = 0x10;
    private const int VK_CONTROL = 0x11;
    private const int VK_MENU = 0x12;
    private const int HOTKEY_ID_MOUSETHROUGH = 0x1001;
    private const int HOTKEY_ID_TOPMOST = 0x1002;
    private const int HOTKEY_ID_RESET_STATISTIC = 0x1003;

    private readonly HashSet<int> _conflictedHotkeys = new();

    // When true, rely solely on low-level keyboard hook so keystrokes are passed through to other apps.
    private readonly bool _transparentHotkeys = true;
    private AppConfig _config = configManager.CurrentConfig;
    private IntPtr _keyboardHookHandle = IntPtr.Zero;
    private LowLevelKeyboardProc? _keyboardProc;

    private HwndSource? _source;

    public void Start()
    {
        AttachMessageHook();
        AttachLocalKeyFallback();
        RegisterAll();
        configManager.ConfigurationUpdated += OnConfigUpdated;
    }

    public void Stop()
    {
        try
        {
            UnregisterAll();
        }
        finally
        {
            ReleaseKeyboardHook();
            DetachMessageHook();
            configManager.ConfigurationUpdated -= OnConfigUpdated;
        }
    }

    public void UpdateFromConfig(AppConfig config)
    {
        _config = config;
        // Ensure all (un)registration runs on the UI thread owning the window/handle
        var dispatcher = windowManager.DpsStatisticsView.Dispatcher;
        if (dispatcher.CheckAccess())
        {
            // Re-register hotkeys to reflect new key or modifiers
            UnregisterAll();
            RegisterAll();
        }
        else
        {
            dispatcher.Invoke(() =>
            {
                UnregisterAll();
                RegisterAll();
            });
        }
    }

    private void OnConfigUpdated(object? sender, AppConfig e)
    {
        UpdateFromConfig(e);
    }

    private void AttachMessageHook()
    {
        if (_source is not null) return;
        var window = windowManager.DpsStatisticsView; // host window for message pump
        var helper = new WindowInteropHelper(window);
        var handle = helper.EnsureHandle();
        _source = HwndSource.FromHwnd(handle);
        if (_source == null)
        {
            logger.LogWarning(
                "Failed to obtain HwndSource from handle {Handle} for window {WindowType}. Global hotkeys will be unavailable.",
                handle,
                window.GetType().Name);
        }
        else
        {
            _source.AddHook(WndProc);
        }
    }

    private void DetachMessageHook()
    {
        if (_source is null) return;
        _source.RemoveHook(WndProc);
        _source = null;
    }

    private void RegisterAll()
    {
        try
        {
            _conflictedHotkeys.Clear();
            RegisterMouseThroughHotkey();
            RegisterTopmostHotkey();
            RegisterResetDpsStatistic();
            TryReleaseKeyboardHookIfUnused();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "RegisterAll hotkeys failed");
        }
    }

    private void UnregisterAll()
    {
        try
        {
            var hWnd = _source?.Handle ?? IntPtr.Zero;
            if (hWnd != IntPtr.Zero)
            {
                UnregisterHotKey(hWnd, HOTKEY_ID_MOUSETHROUGH);
                UnregisterHotKey(hWnd, HOTKEY_ID_TOPMOST);
                UnregisterHotKey(hWnd, HOTKEY_ID_RESET_STATISTIC);
            }

            _conflictedHotkeys.Clear();
            TryReleaseKeyboardHookIfUnused();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "UnregisterAll hotkeys failed");
        }
    }

    private void RegisterMouseThroughHotkey()
    {
        var key = _config.MouseThroughShortcut.Key;
        var mods = _config.MouseThroughShortcut.Modifiers;
        if (key == Key.None) return;

        var (vk, fsMods) = ToNative(key, mods);
        var hWnd = _source?.Handle ?? IntPtr.Zero;
        if (_transparentHotkeys || hWnd == IntPtr.Zero)
        {
            TrackHotkeyForHook(HOTKEY_ID_MOUSETHROUGH);
            return;
        }

        var success = TryRegisterHotKey(hWnd, HOTKEY_ID_MOUSETHROUGH, fsMods, vk, key, mods, out var error);
        HandleRegistrationResult(success, error, HOTKEY_ID_MOUSETHROUGH);
    }

    private void RegisterTopmostHotkey()
    {
        var key = _config.TopmostShortcut.Key;
        var mods = _config.TopmostShortcut.Modifiers;
        if (key == Key.None) return;

        var (vk, fsMods) = ToNative(key, mods);
        var hWnd = _source?.Handle ?? IntPtr.Zero;
        if (_transparentHotkeys || hWnd == IntPtr.Zero)
        {
            TrackHotkeyForHook(HOTKEY_ID_TOPMOST);
            return;
        }

        var success = TryRegisterHotKey(hWnd, HOTKEY_ID_TOPMOST, fsMods, vk, key, mods, out var error);
        HandleRegistrationResult(success, error, HOTKEY_ID_TOPMOST);
    }

    private void RegisterResetDpsStatistic()
    {
        var key = _config.ClearDataShortcut.Key;
        var mods = _config.ClearDataShortcut.Modifiers;
        if (key == Key.None) return;

        var (vk, fsMods) = ToNative(key, mods);
        var hWnd = _source?.Handle ?? IntPtr.Zero;
        if (_transparentHotkeys || hWnd == IntPtr.Zero)
        {
            TrackHotkeyForHook(HOTKEY_ID_RESET_STATISTIC);
            return;
        }

        var success = TryRegisterHotKey(hWnd, HOTKEY_ID_RESET_STATISTIC, fsMods, vk, key, mods, out var error);
        HandleRegistrationResult(success, error, HOTKEY_ID_RESET_STATISTIC);
    }

    private static (uint vk, uint fsMods) ToNative(Key key, ModifierKeys mods)
    {
        var vk = (uint)KeyInterop.VirtualKeyFromKey(key);
        uint fs = 0;
        if (mods.HasFlag(ModifierKeys.Alt)) fs |= 0x0001; // MOD_ALT
        if (mods.HasFlag(ModifierKeys.Control)) fs |= 0x0002; // MOD_CONTROL
        if (mods.HasFlag(ModifierKeys.Shift)) fs |= 0x0004; // MOD_SHIFT
        // ignore windows key by design
        return (vk, fs);
    }

    private bool TryRegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk, Key key, ModifierKeys mods,
        out int lastError, [CallerMemberName] string? name = null)
    {
        // Always attempt to unregister first (safe even if not registered)
        UnregisterHotKey(hWnd, id);
        if (!RegisterHotKey(hWnd, id, fsModifiers, vk))
        {
            lastError = Marshal.GetLastWin32Error();
            logger.LogWarning("RegisterHotKey failed for {Name}: {Key}+{Mods}. Win32Error={Error}", name, key, mods,
                lastError);
            return false;
        }

        lastError = 0;
        return true;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg != WM_HOTKEY) return IntPtr.Zero;

        var id = wParam.ToInt32();
        handled = HandleHotkey(id);

        return IntPtr.Zero;
    }

    private bool HandleHotkey(int id)
    {
        Debug.WriteLine("KeyPressed Hotkey ID: " + id);
        switch (id)
        {
            case HOTKEY_ID_MOUSETHROUGH:
                ToggleMouseThrough();
                return true;
            case HOTKEY_ID_TOPMOST:
                ToggleTopmost();
                return true;
            case HOTKEY_ID_RESET_STATISTIC:
                TriggerReset();
                return true;
            default:
                return false;
        }
    }

    private void ToggleMouseThrough()
    {
        try
        {
            var newState = !_config.MouseThroughEnabled;
            _config.MouseThroughEnabled = newState;
            MouseThroughHelper.ApplyToCoreWindows(_config, windowManager, mousePenetration);
            _ = configManager.SaveAsync(_config); // persist asynchronously
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "ToggleMouseThrough failed");
        }
    }

    private void ToggleTopmost()
    {
        try
        {
            var dpsWindow = windowManager.DpsStatisticsView;
            var personalWindow = windowManager.PersonalDpsView;

            topmostService.ToggleTopmost(dpsWindow);
            topmostService.ToggleTopmost(personalWindow);

            // 保存配置
            _config.TopmostEnabled = dpsWindow.Topmost;
            _ = configManager.SaveAsync(_config);

            logger.LogInformation("TopMostService: Top most state changed to {State}", _config.TopmostEnabled ? "Enabled" : "Disabled");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "ToggleTopmost failed");
        }
    }

    private void TriggerReset()
    {
        try
        {
            personalDpsViewModel.Clear();
            dpsStatisticsViewModel.ResetAll();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "TriggerReset failed");
        }
    }

    private void HandleRegistrationResult(bool success, int error, int id)
    {
        if (success)
        {
            TrackHotkeyForHook(id, true);
            TryReleaseKeyboardHookIfUnused();
            return;
        }

        _conflictedHotkeys.Add(id);
        EnsureKeyboardHook();

        if (error == ERROR_HOTKEY_ALREADY_REGISTERED)
        {
            logger.LogWarning("Hotkey {Id} is already registered by another program. Falling back to keyboard hook.",
                id);
        }
        else
        {
            logger.LogWarning("Hotkey {Id} registration failed (Win32Error={Error}). Falling back to keyboard hook.",
                id, error);
        }
    }

    private void AttachLocalKeyFallback()
    {
        try
        {
            var window = windowManager.DpsStatisticsView;
            window.PreviewKeyDown -= OnLocalPreviewKeyDown;
            window.PreviewKeyDown += OnLocalPreviewKeyDown;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "AttachLocalKeyFallback failed");
        }
    }

    private void OnLocalPreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Local fallback only when hotkey conflicts exist; works when window has focus.
        if (_conflictedHotkeys.Count == 0) return;

        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        var mods = Keyboard.Modifiers;

        if (IsHotkeyMatch(HOTKEY_ID_MOUSETHROUGH, key, mods) && _conflictedHotkeys.Contains(HOTKEY_ID_MOUSETHROUGH))
        {
            e.Handled = HandleHotkey(HOTKEY_ID_MOUSETHROUGH);
            return;
        }

        if (IsHotkeyMatch(HOTKEY_ID_TOPMOST, key, mods) && _conflictedHotkeys.Contains(HOTKEY_ID_TOPMOST))
        {
            e.Handled = HandleHotkey(HOTKEY_ID_TOPMOST);
            return;
        }

        if (IsHotkeyMatch(HOTKEY_ID_RESET_STATISTIC, key, mods) &&
            _conflictedHotkeys.Contains(HOTKEY_ID_RESET_STATISTIC))
        {
            e.Handled = HandleHotkey(HOTKEY_ID_RESET_STATISTIC);
        }
    }

    private bool IsHotkeyMatch(int id, Key key, ModifierKeys mods)
    {
        var binding = id switch
        {
            HOTKEY_ID_MOUSETHROUGH => _config.MouseThroughShortcut,
            HOTKEY_ID_TOPMOST => _config.TopmostShortcut,
            HOTKEY_ID_RESET_STATISTIC => _config.ClearDataShortcut,
            _ => new KeyBinding(Key.None, ModifierKeys.None)
        };

        if (binding.Key == Key.None) return false;
        return binding.Key == key && binding.Modifiers == mods;
    }

    private void EnsureKeyboardHook()
    {
        if (_keyboardHookHandle != IntPtr.Zero) return;

        try
        {
            _keyboardProc ??= LowLevelKeyboardCallback;
            using var process = Process.GetCurrentProcess();
            using var module = process.MainModule;
            var hModule = module != null ? GetModuleHandle(module.ModuleName) : IntPtr.Zero;
            _keyboardHookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardProc, hModule, 0);
            if (_keyboardHookHandle == IntPtr.Zero)
            {
                var error = Marshal.GetLastWin32Error();
                logger.LogWarning("Failed to install keyboard hook. Win32Error={Error}", error);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "EnsureKeyboardHook failed");
        }
    }

    private void TryReleaseKeyboardHookIfUnused()
    {
        if (_transparentHotkeys) return;
        if (_conflictedHotkeys.Count == 0)
        {
            ReleaseKeyboardHook();
        }
    }

    private void TrackHotkeyForHook(int id, bool clearExisting = false)
    {
        if (clearExisting)
        {
            _conflictedHotkeys.Remove(id);
        }

        // In transparent mode we always handle via hook to pass keystrokes through.
        if (_transparentHotkeys)
        {
            _conflictedHotkeys.Add(id);
            EnsureKeyboardHook();
        }
    }

    private void ReleaseKeyboardHook()
    {
        if (_keyboardHookHandle == IntPtr.Zero) return;

        try
        {
            UnhookWindowsHookEx(_keyboardHookHandle);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "ReleaseKeyboardHook failed");
        }
        finally
        {
            _keyboardHookHandle = IntPtr.Zero;
        }
    }

    private IntPtr LowLevelKeyboardCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN) && _conflictedHotkeys.Count > 0)
        {
            var info = Marshal.PtrToStructure<KbdLlHookStruct>(lParam);
            var key = KeyInterop.KeyFromVirtualKey((int)info.vkCode);
            var mods = GetCurrentModifiers();

            if (_conflictedHotkeys.Contains(HOTKEY_ID_MOUSETHROUGH) && IsHotkeyMatch(HOTKEY_ID_MOUSETHROUGH, key, mods))
            {
                HandleHotkey(HOTKEY_ID_MOUSETHROUGH);
            }
            else if (_conflictedHotkeys.Contains(HOTKEY_ID_TOPMOST) && IsHotkeyMatch(HOTKEY_ID_TOPMOST, key, mods))
            {
                HandleHotkey(HOTKEY_ID_TOPMOST);
            }
            else if (_conflictedHotkeys.Contains(HOTKEY_ID_RESET_STATISTIC) &&
                     IsHotkeyMatch(HOTKEY_ID_RESET_STATISTIC, key, mods))
            {
                HandleHotkey(HOTKEY_ID_RESET_STATISTIC);
            }
        }

        return CallNextHookEx(_keyboardHookHandle, nCode, wParam, lParam);
    }

    private static ModifierKeys GetCurrentModifiers()
    {
        var mods = ModifierKeys.None;
        if (IsKeyPressed(VK_CONTROL)) mods |= ModifierKeys.Control;
        if (IsKeyPressed(VK_MENU)) mods |= ModifierKeys.Alt;
        if (IsKeyPressed(VK_SHIFT)) mods |= ModifierKeys.Shift;
        return mods;
    }

    private static bool IsKeyPressed(int virtualKey)
    {
        return (GetAsyncKeyState(virtualKey) & 0x8000) != 0;
    }

    [LibraryImport("user32.dll", SetLastError = true, EntryPoint = "RegisterHotKey")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [LibraryImport("user32.dll", SetLastError = true, EntryPoint = "UnregisterHotKey")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool UnregisterHotKey(IntPtr hWnd, int id);

    [LibraryImport("user32.dll", EntryPoint = "SetWindowsHookExW", SetLastError = true)]
    private static partial IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [LibraryImport("user32.dll", SetLastError = true,EntryPoint = "UnhookWindowsHookEx")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool UnhookWindowsHookEx(IntPtr hhk);

    [LibraryImport("user32.dll", SetLastError = true, EntryPoint = "CallNextHookEx")]
    private static partial IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [LibraryImport("user32.dll", EntryPoint = "GetAsyncKeyState")]
    private static partial short GetAsyncKeyState(int vKey);

    [LibraryImport("kernel32.dll", EntryPoint = "GetModuleHandleW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    private static partial IntPtr GetModuleHandle(string? lpModuleName);

    [StructLayout(LayoutKind.Sequential)]
    private struct KbdLlHookStruct
    {
        public uint vkCode;
        public uint scanCode;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
}