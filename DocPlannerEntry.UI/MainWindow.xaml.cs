using DocPlannerEntry.SlotManagement.Model.Availability;
using DocPlannerEntry.SlotManagement.Model.TakeSlot;

using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace DocPlannerEntry.UI;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public HttpClient httpClient { get; set; }

    public ObservableCollection<SlotUI> slotUIs { get; set; }

    public MainWindow()
    {
        InitializeComponent();

        slotUIs = new ObservableCollection<SlotUI>();

        httpClient = new HttpClient()
        {
            BaseAddress = new Uri("http://localhost:5240")
        };
    }

    private async void ReserveSlot_Button_Click(object sender, RoutedEventArgs e)
    {
        var slotReservationRequest = new SlotReservationRequest()
        {
            FacilityId = new Guid(FacilityIdTb.Text),
            Comments = CommentsTb.Text,
            Start = DateTimeOffset.Parse(StartDate.Text),
            End = DateTimeOffset.Parse(EndDate.Text),
            Patient = new Patient()
            {
                Name = NameTb.Text,
                SecondName = SurnameTb.Text,
                Email = EmailTb.Text,
                Phone = PhoneTb.Text
            }
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/SlotManagement")
        {
            Content = new StringContent(JsonSerializer.Serialize(slotReservationRequest), Encoding.UTF8, "application/json")
        };

        var response = await httpClient.SendAsync(httpRequest);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
            MessageBox.Show("Slot reserved successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        else
            MessageBox.Show("There was an issue reserving the slot", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private async void Load_Button_Click(object sender, RoutedEventArgs e)
    {
        slotUIs.Clear();

        var datePickerValue = LoadDatePicker.ToString();

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/SlotManagement?requestedDate={LoadDatePicker.ToString()}");

        var response = await httpClient.SendAsync(httpRequest);
        var responseBody = await response.Content.ReadAsStringAsync();
        var deserializedSlots = JsonSerializer.Deserialize<List<Slot>>(responseBody);

        slotUIs.AddRange(deserializedSlots);

        this.DataContext = this;
    }
}