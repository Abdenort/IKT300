    using System.Windows.Forms;
    using PsuControlApp;

public class PsuInterfaceForm : Form
{
    private Label deviceTypeValue;
    private Label serialNumberValue;
    private Label maxVoltageValue;
    private Label currentVoltageValue;
    private Label articleNumberValue;

    private TextBox voltageInput;

    private ComboBox psuSelector;

    private readonly PsuInterfaceMqttClient mqtt = new PsuInterfaceMqttClient("127.0.0.1", "test");

    public PsuInterfaceForm()
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

        psuSelector = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        psuSelector.Items.Add("Dummy PSU");
        psuSelector.Items.Add("Physical PSU");

        layout.Controls.Add(new Label { Text = "Select PSU:" }, 0,9);
        layout.Controls.Add(psuSelector, 1, 9);

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

        mqtt.MessageReceived += Mqtt_MessageReceived;

        psuSelector.SelectedIndexChanged += psuSelector_SelectedIndexChanged;

        FormClosed += (sender, e) => mqtt.MqttDisconnect();
    }

    private void MainForm_Load(object? sender, EventArgs e)
    {
        try
        {
            mqtt.MqttConnect();
            mqtt.MqttSubscribe();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void PowerToggleChanged(object? sender, EventArgs e)
    {
        CheckBox toggle = (CheckBox)sender!;

        if (toggle.Checked)
        {
            mqtt.MqttPublish("powerOn", "");
        }
        else
        {
            mqtt.MqttPublish("powerOff", "");
        }
    }

    private void RemoteToggleChanged(object? sender, EventArgs e)
    {
        CheckBox toggle = (CheckBox)sender!;

        if (toggle.Checked)
        {
            mqtt.MqttPublish("remoteOn", "");
        }
        else
        {
            mqtt.MqttPublish("remoteOff", "");
        }
    }

    private void GetVoltage(object? sender, EventArgs e)
    {
        try
        {
            mqtt.MqttPublish("getVoltage", "");
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

        mqtt.MqttPublish(
            "setVoltage",
            voltage.ToString(System.Globalization.CultureInfo.InvariantCulture)
            );
    }

    private void Mqtt_MessageReceived(string status, string message)
    {
        if (IsDisposed || !IsHandleCreated)
            return;

        BeginInvoke(new Action(() =>
        {
            switch (status)
            {
                case "deviceType":
                    deviceTypeValue.Text = message;
                    break;

                case "serialNumber":
                    serialNumberValue.Text = message;
                    break;

                case "articleNumber":
                    articleNumberValue.Text = message;
                    break;

                case "maxVoltage":
                    maxVoltageValue.Text = message + "V";
                    break;

                case "voltage":
                    currentVoltageValue.Text = message + "V";
                    break;
            }
        }));
    }

    private void psuSelector_SelectedIndexChanged(object? sender, EventArgs e) 
    {
        int psuType = psuSelector.SelectedIndex;

        if (psuType < 0)
            return;

        mqtt.MqttPublish("select", psuType.ToString());
        mqtt.MqttPublish("getInfo", "");
    }
}