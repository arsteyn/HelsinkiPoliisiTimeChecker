namespace PoliisiAppointment;

public class ApiResponce
{
    public string prereservation { get; set; }

    public DateTime siteMargin { get; set; }

    public Dictionary<DateTime, Slot> slots { get; set; }
}

public class Slot
{
    public IList<DateTime> timeSlots { get; set; }
    public bool closed { get; set; }
}