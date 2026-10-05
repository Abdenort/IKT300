using System.Windows.Forms;

public class MainForm : Form
{
    private Label deviceTypeValue;
    private Label serialNumberValue;
    private Label maxVoltageValue;
    private Label currentVoltageValue;
    private Label articleNumberValue;

    private TextBox voltageInput;

    public MainForm()
    {

        Text = "PS2000 Control";
        Width = 500;
        Height = 450;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 10,
            Padding = new Padding(20),
            AutoSize = true
        };

        deviceTypeValue = new Label { Text = "-", AutoSize = true };
        serialNumberValue = new Label { Text = "-", AutoSize = true };
        maxVoltageValue = new Label { Text = "-", AutoSize = true };
        currentVoltageValue = new Label { Text = "-", AutoSize = true };
        articleNumberValue = new Label { Text = "-", AutoSize = true };

        layout.Controls.Add(new Label { Text = "Device type:" }, 0, 0);
        layout.Controls.Add(deviceTypeValue, 1, 0);

        layout.Controls.Add(new Label { Text = "Serial number:" }, 0, 1);
        layout.Controls.Add(serialNumberValue, 1, 1);

        layout.Controls.Add(new Label { Text = "Article number:" }, 0, 2);
        layout.Controls.Add(articleNumberValue, 1, 2);

        layout.Controls.Add(new Label { Text = "Maximum voltage:" }, 0, 3);
        layout.Controls.Add(maxVoltageValue, 1, 3);

        layout.Controls.Add(new Label { Text = "Current voltage:" }, 0, 4);
        layout.Controls.Add(currentVoltageValue, 1, 4);

        voltageInput = new TextBox();

        layout.Controls.Add(new Label { Text = "Set voltage:" }, 0, 5);
        layout.Controls.Add(voltageInput, 1, 5);

        var getVoltageButton = new Button { Text = "Get voltage" };
        var setVoltageButton = new Button { Text = "Set voltage" };

        layout.Controls.Add(getVoltageButton, 0, 6);
        layout.Controls.Add(setVoltageButton, 1, 6);

        var powerToggle = new CheckBox
        {
            Text = "ON / OFF",
            AutoSize = true
        };

        var remoteToggle = new CheckBox
        {
            Text = "ON / OFF",
            AutoSize = true
        };

        layout.Controls.Add(new Label { Text = "Remote control:" }, 0, 7);
        layout.Controls.Add(remoteToggle, 1, 7);

        layout.Controls.Add(new Label { Text = "Power output:" }, 0, 8);
        layout.Controls.Add(powerToggle, 1, 8);

        Controls.Add(layout);

        Load += MainForm_Load;

        powerToggle.CheckedChanged += PowerToggleChanged;
        remoteToggle.CheckedChanged += RemoteToggleChanged;

        getVoltageButton.Click += GetVoltage;
        setVoltageButton.Click += SetVoltage;
    }

    private void MainForm_Load(object? sender, EventArgs e)
    {
        try
        {
            Program.Test2();
            currentVoltageValue.Text = Program.currentVoltage + "V";
            maxVoltageValue.Text = Program.MaxVoltage + "V";
            deviceTypeValue.Text = Program.DeviceType;
            serialNumberValue.Text = Program.SerialNumber;
            articleNumberValue.Text = Program.ArticleNumber;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            deviceTypeValue.Text = "Error: " + ex.Message;
            currentVoltageValue.Text = "Error: " + ex.Message;
            maxVoltageValue.Text = "Error: " + ex.Message;
            serialNumberValue.Text = "Error: " + ex.Message;
            articleNumberValue.Text = "Error: " + ex.Message;

        }
    }

    private void PowerToggleChanged(object? sender, EventArgs e)
    {
        CheckBox toggle = (CheckBox)sender!;

        if (toggle.Checked)
        {
            Program.PowerOutputOn();
        }
        else
        {
            Program.PowerOutputOff();
        }
    }

    private void RemoteToggleChanged(object? sender, EventArgs e)
    {
        CheckBox toggle = (CheckBox)sender!;

        if (toggle.Checked)
        {
            Program.RemoteControlOn();
        }
        else
        {
            Program.RemoteControlOff();
        }
    }
    
    private void GetVoltage(object? sender, EventArgs e)
    {
        try
        {
            Program.Test2();
            currentVoltageValue.Text = Program.currentVoltage + "V";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void SetVoltage(object? sender, EventArgs e)
    {
        if (!float.TryParse(voltageInput.Text, out float voltage))
        {
            MessageBox.Show("Enter a valid voltage");
            return;
        }

        try
        {
            Program.SetVoltage(voltage);
            Program.Test2();

            currentVoltageValue.Text = Program.currentVoltage + "V";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
}