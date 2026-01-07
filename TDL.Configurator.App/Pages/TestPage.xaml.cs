using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using TDL.Configurator.Core;

namespace TDL.Configurator.App.Pages;

public partial class TestPage : System.Windows.Controls.UserControl
{
    public TestPage()
    {
        InitializeComponent();
    }

    private string GamePath => AppSettings.Load().GamePath.Trim();

    private string SendToolPath =>
        Path.Combine(GamePath, "Data", "TDL", "Tools", "tdl_send.exe");

    // 0->1, 1->2, 2->3
    private string GetSourceArg()
    {
        var idx = SourceBox?.SelectedIndex ?? 1;
        return idx switch
        {
            0 => "1",
            2 => "3",
            _ => "2"
        };
    }

    private bool EnsureToolReady()
    {
        if (string.IsNullOrWhiteSpace(GamePath) || !Directory.Exists(GamePath))
        {
            System.Windows.MessageBox.Show(
                "Сначала укажи путь к игре в Настройках.",
                "TDL Configurator",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        if (!File.Exists(SendToolPath))
        {
            System.Windows.MessageBox.Show(
                $"Не найден tdl_send.exe:\n{SendToolPath}",
                "TDL Configurator",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        return true;
    }

    private void PingNormal_Click(object sender, RoutedEventArgs e)
        => RunToolNormal("SYSTEM_PING");

    private void HealingNormal_Click(object sender, RoutedEventArgs e)
        => RunToolNormal("SYSTEM_HEALING");

    private void HealingForce1_Click(object sender, RoutedEventArgs e)
        => RunToolForce1("SYSTEM_HEALING");

    private void ScatterNormal_Click(object sender, RoutedEventArgs e)
        => RunToolNormal("INVENTORY_SCATTER");

    private void ScatterForce_Click(object sender, RoutedEventArgs e)
    {
        // INVENTORY_SCATTER: FORCE COUNT <= 10
        if (!TryGetForceParams(maxCount: 10, out var count, out var interval))
            return;

        RunToolForce("INVENTORY_SCATTER", count, interval);
    }
    private void TeleportRandomCity_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        // TELEPORT_RANDOM_CITY: FORCE COUNT <= 5
        if (!TryGetForceParams(maxCount: 5, out var count, out var interval))
            return;

        RunToolForce("TELEPORT_RANDOM_CITY", count, interval);
    }

    private void TeleportRandomDanger_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        // TELEPORT_RANDOM_DANGER: FORCE COUNT <= 5
        if (!TryGetForceParams(maxCount: 5, out var count, out var interval))
            return;

        RunToolForce("TELEPORT_RANDOM_DANGER", count, interval);
    }

    private void VirusDisease_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        // VIRUS_DISEASE: FORCE COUNT <= 15
        if (!TryGetForceParams(maxCount: 15, out var count, out var interval))
            return;

        RunToolForce("VIRUS_DISEASE", count, interval);
    }

    private void RunToolNormal(string action)
    {
        if (!EnsureToolReady()) return;

        var source = GetSourceArg();
        RunProcess(SendToolPath, $"NORMAL {action} {source}");
    }

    private void RunToolForce1(string action)
    {
        if (!EnsureToolReady()) return;

        RunProcess(SendToolPath, $"FORCE1 {action}");
    }

    private void RunToolForce(string action, int count, string? interval)
    {
        if (!EnsureToolReady()) return;

        var args = interval is null
            ? $"FORCE {action} {count}"
            : $"FORCE {action} {count} {interval}";

        RunProcess(SendToolPath, args);
    }

    private bool TryGetForceParams(int maxCount, out int count, out string? intervalArg)
    {
        count = 1;
        intervalArg = null;

        var countText = (ForceCountBox?.Text ?? "").Trim();
        if (!int.TryParse(countText, NumberStyles.Integer, CultureInfo.InvariantCulture, out count) &&
            !int.TryParse(countText, NumberStyles.Integer, CultureInfo.CurrentCulture, out count))
        {
            System.Windows.MessageBox.Show("COUNT должен быть целым числом.", "TDL Configurator",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (count < 1) count = 1;
        if (count > maxCount)
        {
            System.Windows.MessageBox.Show($"COUNT превышает лимит для этой команды (max {maxCount}).", "TDL Configurator",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        var intervalText = (ForceIntervalBox?.Text ?? "").Trim();
        if (string.IsNullOrWhiteSpace(intervalText))
        {
            intervalArg = null; // не передаём — берётся default из TDL_Cooldowns.ini
            return true;
        }

        if (!TryParseDoubleFlexible(intervalText, out var interval))
        {
            System.Windows.MessageBox.Show("INTERVAL должен быть числом (пример: 0.75).", "TDL Configurator",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        // общий clamp из протокола: 0.75..5.0
        if (interval < 0.75) interval = 0.75;
        if (interval > 5.0) interval = 5.0;

        intervalArg = interval.ToString("0.##", CultureInfo.InvariantCulture);
        return true;
    }

    private static bool TryParseDoubleFlexible(string s, out double value)
    {
        if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            return true;

        if (double.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
            return true;

        var swapped = s.Contains(',') ? s.Replace(',', '.') : s.Replace('.', ',');
        if (double.TryParse(swapped, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            return true;

        value = 0;
        return false;
    }

    private void RunProcess(string exePath, string arguments)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using var p = Process.Start(psi);
            if (p == null)
            {
                Append($"[{DateTime.Now:HH:mm:ss}] Не удалось запустить процесс.\n");
                return;
            }

            var stdout = p.StandardOutput.ReadToEnd();
            var stderr = p.StandardError.ReadToEnd();
            p.WaitForExit();

            Append($"[{DateTime.Now:HH:mm:ss}] CMD: {Path.GetFileName(exePath)} {arguments}\n");
            Append($"ExitCode: {p.ExitCode}\n");

            if (!string.IsNullOrWhiteSpace(stdout))
                Append($"STDOUT:\n{stdout}\n");

            if (!string.IsNullOrWhiteSpace(stderr))
                Append($"STDERR:\n{stderr}\n");
        }
        catch (Exception ex)
        {
            Append($"[{DateTime.Now:HH:mm:ss}] ERROR: {ex.Message}\n");
        }
    }

    private void Append(string text)
    {
        OutputBox.AppendText(text);
        OutputBox.ScrollToEnd();
    }
}
